using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EcommerceApp.DAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

// 始终启用 Swagger
app.UseSwagger();
app.UseSwaggerUI();

// ----------------- Auth Endpoints -----------------
app.MapPost("/api/auth/register", (RegisterDto dto, UserDalSqlite dal) =>
{
    if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        return Results.BadRequest("邮箱与密码不能为空");

    var hash = Sha256(dto.Password);
    var id = dal.RegisterUser(dto.Email, dto.Phone, hash, dto.NickName ?? "新用户");
    return id > 0 ? Results.Ok(new { id }) : Results.Conflict("注册失败");
});

app.MapPost("/api/auth/login", (LoginDto dto, UserDalSqlite dal) =>
{
    var hash = Sha256(dto.Password);
    var user = dal.ValidateLogin(dto.Identifier, hash);
    if (user == null) return Results.Unauthorized();

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
    dal.AddItem(uid, dto.ProductId, dto.Quantity);
    return Results.Ok();
}).RequireAuthorization();

app.MapDelete("/api/cart/{productId}", (long productId, ClaimsPrincipal user, CartDalSqlite dal) =>
{
    if (!long.TryParse(user.FindFirstValue("uid"), out var uid))
        return Results.Unauthorized();
    dal.RemoveItem(uid, productId);
    return Results.Ok();
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
