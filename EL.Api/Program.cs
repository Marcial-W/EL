using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EcommerceApp.DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ----------------- 基础配置 -----------------
// 先读取环境变量 SQLITE_DB_PATH，若未设置则默认当前目录 ecommerce.db
var connStr = Environment.GetEnvironmentVariable("SQLITE_DB_PATH") ?? "Data Source=./ecommerce.db";
SqliteInitializer.EnsureCreated(connStr);

var jwtKey = builder.Configuration["JwtKey"] ?? "ElectroShop_SuperJwtKey_2026!@#$%";
var issuer = "ElectroShop";
var audience = "ElectroShopUsers";

// ----------------- DI -----------------
builder.Services.AddSingleton(new UserDalSqlite(connStr));
builder.Services.AddSingleton(new CartDalSqlite(connStr));
builder.Services.AddSingleton(new OrderDalSqlite(connStr));

// ----------------- JWT -----------------
var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
var signingKey = new SymmetricSecurityKey(keyBytes);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    });

builder.Services.AddAuthorization();

// Swagger & CORS
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// 配置静态文件服务，使用 EL 文件夹作为静态文件目录
var elFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "EL");
var staticFileOptions = new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(elFolderPath),
    RequestPath = ""
};
app.UseDefaultFiles(new DefaultFilesOptions
{
    FileProvider = staticFileOptions.FileProvider
});
app.UseStaticFiles(staticFileOptions);

// 始终启用 Swagger
app.UseSwagger();
app.UseSwaggerUI();

// ----------------- Auth Endpoints -----------------
app.MapPost("/api/auth/register", (RegisterDto dto, UserDalSqlite dal) =>
{
    // 验证必填字段
    if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        return Results.BadRequest("邮箱与密码不能为空");
    
    // 验证用户名（必填）
    if (string.IsNullOrWhiteSpace(dto.NickName))
        return Results.BadRequest("用户名不能为空");

    // 验证邮箱格式（简单验证）
    if (!dto.Email.Contains("@") || !dto.Email.Contains("."))
        return Results.BadRequest("邮箱格式不正确");

    // 验证密码长度
    if (dto.Password.Length < 8)
        return Results.BadRequest("密码长度至少为8位");

    try
    {
        var hash = Sha256(dto.Password);
        var id = dal.RegisterUser(dto.Email, dto.Phone, hash, dto.NickName);
        
        if (id > 0)
        {
            // 注册成功，返回用户ID
            return Results.Ok(new { id, message = "注册成功" });
        }
        else
        {
            // 注册失败，可能是邮箱或手机号已存在
            return Results.Conflict("注册失败，邮箱或手机号可能已被使用");
        }
    }
    catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19) // UNIQUE constraint failed
    {
        // SQLite唯一约束违反（邮箱或手机号已存在）
        return Results.Conflict("邮箱或手机号已被注册");
    }
    catch (Exception ex)
    {
        // 其他异常
        return Results.Problem($"注册时发生错误: {ex.Message}");
    }
});

app.MapPost("/api/auth/login", (LoginDto dto, UserDalSqlite dal) =>
{
    // 验证必填字段
    if (string.IsNullOrWhiteSpace(dto.Identifier) || string.IsNullOrWhiteSpace(dto.Password))
        return Results.BadRequest("请输入邮箱/手机号和密码");

    // 先检查用户是否存在
    if (!dal.UserExists(dto.Identifier))
    {
        return Results.NotFound("用户不存在，请注册");
    }

    // 验证密码
    var hash = Sha256(dto.Password);
    var user = dal.ValidateLogin(dto.Identifier, hash);
    
    if (user == null)
    {
        // 用户存在但密码错误
        return Results.Unauthorized();
    }

    // 登录成功，生成token并返回用户信息
    var token = GenerateJwtToken(user.UserId);
    return Results.Ok(new { token, user });
});

// ----------------- Cart Endpoints -----------------
app.MapGet("/api/cart", (ClaimsPrincipal user, CartDalSqlite dal) =>
{
    if (!long.TryParse(user.FindFirstValue("uid"), out var uid))
        return Results.Unauthorized();
    var items = dal.GetCart(uid);
    return Results.Ok(items);
}).RequireAuthorization();

app.MapPost("/api/cart/add", (AddCartDto dto, ClaimsPrincipal user, CartDalSqlite dal) =>
{
    if (!long.TryParse(user.FindFirstValue("uid"), out var uid))
        return Results.Unauthorized();
    try
    {
        dal.AddItem(uid, dto.ProductId, dto.Quantity);
        return Results.Ok(new { success = true, message = "已加入购物车" });
    }
    catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.SqliteErrorCode == 19)
    {
        // 外键约束失败，商品不存在
        return Results.BadRequest($"商品ID {dto.ProductId} 不存在");
    }
    catch (Exception ex)
    {
        return Results.Problem($"添加购物车失败: {ex.Message}");
    }
}).RequireAuthorization();

app.MapDelete("/api/cart/{productId}", (long productId, ClaimsPrincipal user, CartDalSqlite dal) =>
{
    if (!long.TryParse(user.FindFirstValue("uid"), out var uid))
        return Results.Unauthorized();
    dal.RemoveItem(uid, productId);
    return Results.Ok(new { success = true, message = "已删除" });
}).RequireAuthorization();

// ----------------- Order Endpoints -----------------
// 创建订单（从购物车）
app.MapPost("/api/orders/create", (ClaimsPrincipal user, CartDalSqlite cartDal, OrderDalSqlite orderDal) =>
{
    if (!long.TryParse(user.FindFirstValue("uid"), out var uid))
        return Results.Unauthorized();
    
    // 获取购物车商品
    var cartItems = cartDal.GetCart(uid).ToList();
    if (cartItems.Count == 0)
        return Results.BadRequest("购物车为空，无法创建订单");
    
    try
    {
        var orderId = orderDal.CreateOrder(uid, cartItems);
        if (orderId > 0)
        {
            // 清空购物车
            foreach (var item in cartItems)
            {
                cartDal.RemoveItem(uid, item.ProductId);
            }
            return Results.Ok(new { orderId, message = "订单创建成功" });
        }
        return Results.Problem("创建订单失败");
    }
    catch (Exception ex)
    {
        return Results.Problem($"创建订单时发生错误: {ex.Message}");
    }
}).RequireAuthorization();

// 获取用户的所有订单
app.MapGet("/api/orders", (ClaimsPrincipal user, OrderDalSqlite dal) =>
{
    if (!long.TryParse(user.FindFirstValue("uid"), out var uid))
        return Results.Unauthorized();
    var orders = dal.GetOrders(uid);
    return Results.Ok(orders);
}).RequireAuthorization();

// 获取订单详情
app.MapGet("/api/orders/{orderId}", (long orderId, ClaimsPrincipal user, OrderDalSqlite dal) =>
{
    if (!long.TryParse(user.FindFirstValue("uid"), out var uid))
        return Results.Unauthorized();
    var order = dal.GetOrderDetail(uid, orderId);
    if (order == null)
        return Results.NotFound("订单不存在");
    return Results.Ok(order);
}).RequireAuthorization();

app.Run();

// ----------------- Helpers -----------------
string GenerateJwtToken(long uid)
{
    var claims = new[]
    {
        new Claim("uid", uid.ToString())
    };
    var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
    var token = new JwtSecurityToken(issuer, audience, claims, expires: DateTime.UtcNow.AddHours(6), signingCredentials: creds);
    return new JwtSecurityTokenHandler().WriteToken(token);
}

static string Sha256(string raw)
{
    using var sha = SHA256.Create();
    var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
    var sb = new StringBuilder();
    foreach (var b in bytes) sb.Append(b.ToString("x2"));
    return sb.ToString();
}

// ----------------- DTO -----------------
public record RegisterDto(string Email, string? Phone, string Password, string? NickName);
public record LoginDto(string Identifier, string Password);
public record AddCartDto(long ProductId, int Quantity);
