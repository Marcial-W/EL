# 电子产品电商教学项目

本仓库包含前后端完整示例，便于课程实践演示。

## 目录结构

| 目录 | 说明 |
|------|------|
| `EL` | 前端静态页面与公共脚本/样式 |
| `EL.Api` | ASP.NET Core Minimal API（SQLite 持久化） |
| `api_test.py` | 自动化接口排查脚本 |

## 快速启动（本地）

> 需先安装 .NET 7 SDK 与 Python 3。

```bash
# 进入后端目录
cd 系统实现/EL.Api
# 还原依赖并运行
dotnet restore
dotnet run
```
运行成功后监听 `http://localhost:5000`，浏览器访问：

* `http://localhost:5000/swagger` 查看接口文档
* `http://localhost:5000/api` 下各端点参见 Swagger

SQLite 数据库文件默认生成在 `EL.Api` 目录下 `ecommerce.db`。

### 接口快速测试

```bash
python ../api_test.py  # 或直接双击运行
```
脚本将自动完成注册→登录→购物车增删查流程。

## 前端页面预览

直接双击打开 `EL/index.html` 即可；主要页面：

* `index.html` 首页 + 商品卡片 + 分类过滤
* `detail.html` 商品详情（示例）
* `login.html` / `register.html` 用户认证
* `cart.html` 购物车，已与后端联调
* `order.html` 订单页面（待后续扩展）

> 前端默认使用 `http://localhost:5000/api`，若后端端口修改请同步更新 `scripts.js` 中 `API_BASE`。

## 主要技术栈

* ASP.NET Core 7 Minimal API + JWT + SQLite
* HTML5 / CSS3 / 原生 JavaScript
* Swagger (Swashbuckle) 自动文档

## 常见问题

1. **JWT 密钥过短报错**  
   修改 `Program.cs` 中 `jwtKey` 环境变量，保证 ≥ 32 字符。
2. **SQLite 外键失败**  
   初始化脚本已插入 `ProductId = 101` 测试数据。如需更多商品，可自行插入。

## TODO

- 订单接口与前端评价流程
- 商品列表后端分页与搜索
- 前端 UI 优化（加载动画、Toast 提示等）

