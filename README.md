# ElectroShop - 电子商务应用

一个基于 ASP.NET Core 和原生前端技术的现代化电子商务平台，提供用户注册登录、商品浏览、购物车管理和订单处理等完整的电商功能。

## 📋 目录

- [项目简介](#项目简介)
- [技术栈](#技术栈)
- [功能特性](#功能特性)
- [项目结构](#项目结构)
- [环境要求](#环境要求)
- [快速开始](#快速开始)
- [数据库结构](#数据库结构)
- [API 文档](#api-文档)
- [前端页面](#前端页面)
- [认证机制](#认证机制)
- [开发说明](#开发说明)

## 🎯 项目简介

ElectroShop 是一个全栈电子商务应用，采用前后端分离的架构设计：

- **后端**: ASP.NET Core 7.0 Web API，提供 RESTful API 接口
- **前端**: 原生 HTML/CSS/JavaScript，无需框架依赖
- **数据库**: SQLite，轻量级、易部署
- **认证**: JWT (JSON Web Tokens) 基于 Token 的身份验证

## 🛠 技术栈

### 后端

- **.NET 7.0** - 跨平台应用框架
- **ASP.NET Core Web API** - RESTful API 服务
- **SQLite** - 嵌入式关系型数据库
- **JWT Bearer Authentication** - 身份验证中间件
- **Swagger** - API 文档和测试工具

### 前端

- **HTML5** - 页面结构
- **CSS3** - 现代化样式设计
- **JavaScript (ES6+)** - 客户端逻辑
- **Fetch API** - HTTP 请求
- **LocalStorage** - 本地存储

## ✨ 功能特性

### 用户管理

- ✅ 用户注册（邮箱/手机号 + 用户名 + 密码）
- ✅ 用户登录（支持邮箱或手机号登录）
- ✅ JWT Token 身份验证
- ✅ 用户信息存储和管理

### 商品管理

- ✅ 商品列表浏览（首页）
- ✅ 商品分类筛选
- ✅ 商品详情查看
- ✅ 15+ 款电子产品展示

### 购物车功能

- ✅ 添加商品到购物车
- ✅ 查看购物车列表
- ✅ 修改商品数量
- ✅ 删除购物车商品
- ✅ 用户专属购物车（仅查看自己的商品）

### 订单管理

- ✅ 从购物车创建订单
- ✅ 查看订单列表
- ✅ 查看订单详情
- ✅ 订单状态管理
- ✅ 用户专属订单（仅查看自己的订单）

### 用户体验

- ✅ 响应式设计，适配多种屏幕
- ✅ 简洁大气的 UI 设计
- ✅ 友好的错误提示
- ✅ 页面间流畅导航
- ✅ 未登录用户自动跳转登录

## 📁 项目结构

```
系统实现/
├── EL/                          # 前端静态文件目录
│   ├── index.html              # 首页
│   ├── detail.html             # 商品详情页
│   ├── cart.html               # 购物车页面
│   ├── order.html              # 订单页面
│   ├── login.html              # 登录页面
│   ├── register.html           # 注册页面
│   ├── scripts.js              # 前端 JavaScript 逻辑
│   ├── style.css               # 全局样式文件
│   ├── *.cs                    # 数据访问层 (DAL) 文件
│   └── *.png, *.jpg            # 图片资源
│
├── EL.Api/                     # 后端 API 项目
│   ├── Program.cs              # 应用程序入口和路由配置
│   ├── EL.Api.csproj           # 项目文件
│   └── ecommerce.db            # SQLite 数据库文件（自动生成）
│
└── README.md                   # 项目文档
```

## 📦 环境要求

- **.NET 7.0 SDK** 或更高版本
- **Windows/Linux/macOS** 任一操作系统
- **现代浏览器**（Chrome、Firefox、Edge、Safari 等）

## 🚀 快速开始

### 1. 克隆项目

```bash
git clone <repository-url>
cd 系统实现
```

### 2. 恢复依赖

```bash
cd EL.Api
dotnet restore
```

### 3. 运行项目

```bash
dotnet run
```

后端服务默认运行在: `http://localhost:5000` 或 `https://localhost:5001`

### 4. 访问应用

打开浏览器访问：

- **首页**: `http://localhost:5000/index.html`
- **API 文档**: `http://localhost:5000/swagger`

### 5. 首次使用

1. 访问注册页面创建新账户
2. 使用注册的邮箱/手机号和密码登录
3. 浏览商品并添加到购物车
4. 在购物车页面创建订单

## 💾 数据库结构

### Users 表

| 字段         | 类型    | 说明                   |
| ------------ | ------- | ---------------------- |
| UserId       | INTEGER | 主键，自增             |
| Email        | TEXT    | 邮箱（唯一）           |
| Phone        | TEXT    | 手机号（唯一，可为空） |
| PasswordHash | TEXT    | 密码哈希（SHA256）     |
| NickName     | TEXT    | 用户名                 |
| Role         | INTEGER | 角色（默认 0）         |
| CreatedAt    | TEXT    | 创建时间               |

### Products 表

| 字段      | 类型    | 说明     |
| --------- | ------- | -------- |
| ProductId | INTEGER | 主键     |
| Name      | TEXT    | 商品名称 |
| Price     | REAL    | 商品价格 |

### ShoppingCarts 表

| 字段   | 类型    | 说明             |
| ------ | ------- | ---------------- |
| CartId | INTEGER | 主键，自增       |
| UserId | INTEGER | 外键，关联 Users |

### CartItems 表

| 字段                      | 类型    | 说明                     |
| ------------------------- | ------- | ------------------------ |
| CartItemId                | INTEGER | 主键，自增               |
| CartId                    | INTEGER | 外键，关联 ShoppingCarts |
| ProductId                 | INTEGER | 外键，关联 Products      |
| Quantity                  | INTEGER | 商品数量                 |
| UNIQUE(CartId, ProductId) |         | 唯一约束                 |

### Orders 表

| 字段        | 类型    | 说明                     |
| ----------- | ------- | ------------------------ |
| OrderId     | INTEGER | 主键，自增               |
| UserId      | INTEGER | 外键，关联 Users         |
| OrderNumber | TEXT    | 订单号（唯一）           |
| TotalAmount | REAL    | 订单总金额               |
| Status      | TEXT    | 订单状态（默认：待付款） |
| CreatedAt   | TEXT    | 创建时间                 |

### OrderItems 表

| 字段        | 类型    | 说明                |
| ----------- | ------- | ------------------- |
| OrderItemId | INTEGER | 主键，自增          |
| OrderId     | INTEGER | 外键，关联 Orders   |
| ProductId   | INTEGER | 外键，关联 Products |
| ProductName | TEXT    | 商品名称（快照）    |
| Price       | REAL    | 商品价格（快照）    |
| Quantity    | INTEGER | 商品数量            |

## 🔌 API 文档

### 认证相关

#### POST /api/auth/register

用户注册

**请求体:**

```json
{
  "email": "user@example.com",
  "phone": "13800138000", // 可选
  "password": "password123",
  "nickName": "用户名" // 必填
}
```

**响应:**

```json
{
  "id": 1,
  "message": "注册成功"
}
```

#### POST /api/auth/login

用户登录

**请求体:**

```json
{
  "identifier": "user@example.com", // 邮箱或手机号
  "password": "password123"
}
```

**响应:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "userId": 1,
    "email": "user@example.com",
    "nickName": "用户名"
  }
}
```

### 购物车相关

#### GET /api/cart

获取当前用户的购物车（需要认证）

**响应:**

```json
[
  {
    "productId": 102,
    "name": "旗舰手机 X1",
    "price": 3999.0,
    "quantity": 1
  }
]
```

#### POST /api/cart/add

添加商品到购物车（需要认证）

**请求体:**

```json
{
  "productId": 102,
  "quantity": 1
}
```

**响应:**

```json
{
  "success": true,
  "message": "已加入购物车"
}
```

#### DELETE /api/cart/{productId}

从购物车删除商品（需要认证）

**响应:**

```json
{
  "success": true,
  "message": "已删除"
}
```

### 订单相关

#### POST /api/orders/create

从购物车创建订单（需要认证）

**响应:**

```json
{
  "orderId": 1,
  "message": "订单创建成功"
}
```

#### GET /api/orders

获取当前用户的所有订单（需要认证）

**响应:**

```json
[
  {
    "orderId": 1,
    "orderNumber": "ORD202601011200001",
    "totalAmount": 4997.0,
    "status": "待付款",
    "createdAt": "2026-01-01 12:00:00"
  }
]
```

#### GET /api/orders/{orderId}

获取订单详情（需要认证）

**响应:**

```json
{
  "orderId": 1,
  "orderNumber": "ORD202601011200001",
  "totalAmount": 4997.0,
  "status": "待付款",
  "createdAt": "2026-01-01 12:00:00",
  "items": [
    {
      "productId": 102,
      "productName": "旗舰手机 X1",
      "price": 3999.0,
      "quantity": 1
    }
  ]
}
```

## 🌐 前端页面

### index.html - 首页

- 展示商品列表
- 支持分类筛选
- 点击商品卡片跳转到详情页

### detail.html - 商品详情页

- 显示商品详细信息
- 添加到购物车功能

### cart.html - 购物车页面

- 显示当前用户的购物车商品
- 修改商品数量
- 删除商品
- 结算功能（创建订单）

### order.html - 订单页面

- 显示当前用户的所有订单
- 查看订单详情

### login.html - 登录页面

- 用户登录表单
- 支持邮箱或手机号登录

### register.html - 注册页面

- 用户注册表单
- 必填字段：邮箱、用户名、密码

## 🔐 认证机制

项目使用 JWT (JSON Web Tokens) 进行身份验证：

1. **登录**: 用户登录后，后端生成 JWT Token
2. **存储**: 前端将 Token 存储在 `localStorage` 中
3. **请求**: 后续 API 请求在 `Authorization` 请求头中携带 Token
   ```
   Authorization: Bearer <token>
   ```
4. **验证**: 后端中间件验证 Token 的有效性
5. **授权**: 受保护的端点需要有效的 Token

**Token 有效期**: 6 小时

**受保护的端点**: 所有 `/api/cart/*` 和 `/api/orders/*` 端点都需要认证

## 💻 开发说明

### 数据库初始化

数据库在首次运行时自动创建。`SqliteInitializer.cs` 负责：

- 创建所有数据表
- 插入初始商品数据（15 款产品）

### 静态文件服务

前端文件位于 `EL/` 目录，通过 ASP.NET Core 静态文件中间件提供服务。

### 密码加密

用户密码使用 SHA256 哈希算法加密后存储。

### CORS 配置

当前配置允许所有来源的跨域请求（开发环境）。生产环境建议限制特定域名。

### 错误处理

- 前端: 使用 `fetchJson` 统一处理 API 响应和错误
- 后端: 返回标准的 HTTP 状态码和错误信息

### 缓存控制

前端资源文件使用版本号参数强制刷新缓存（如 `style.css?v=7`）。

## 📝 注意事项

1. **数据库文件**: `ecommerce.db` 会在首次运行时自动创建，位于 `EL.Api` 目录
2. **JWT Key**: 默认密钥在 `Program.cs` 中，生产环境建议使用环境变量
