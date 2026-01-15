using Microsoft.Data.Sqlite;

namespace EcommerceApp.DAL
{
    public static class SqliteInitializer
    {
        public static void EnsureCreated(string connStr)
        {
            using var con = new SqliteConnection(connStr);
            con.Open();
            var cmd = con.CreateCommand();
            cmd.CommandText = @"
            PRAGMA foreign_keys = ON;
            CREATE TABLE IF NOT EXISTS Users(
                UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                Email TEXT UNIQUE NOT NULL,
                Phone TEXT UNIQUE,
                PasswordHash TEXT NOT NULL,
                NickName TEXT NOT NULL,
                Role INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT DEFAULT (datetime('now'))
            );
            CREATE TABLE IF NOT EXISTS Products(
                ProductId INTEGER PRIMARY KEY,
                Name TEXT NOT NULL,
                Price REAL NOT NULL
            );
            CREATE TABLE IF NOT EXISTS ShoppingCarts(
                CartId INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER UNIQUE,
                FOREIGN KEY(UserId) REFERENCES Users(UserId) ON DELETE CASCADE
            );
            CREATE TABLE IF NOT EXISTS CartItems(
                CartItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                CartId INTEGER,
                ProductId INTEGER,
                Quantity INTEGER NOT NULL,
                UNIQUE(CartId,ProductId),
                FOREIGN KEY(CartId) REFERENCES ShoppingCarts(CartId) ON DELETE CASCADE,
                FOREIGN KEY(ProductId) REFERENCES Products(ProductId)
            );
            CREATE TABLE IF NOT EXISTS Orders(
                OrderId INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                OrderNumber TEXT UNIQUE NOT NULL,
                TotalAmount REAL NOT NULL,
                Status TEXT NOT NULL DEFAULT '待付款',
                CreatedAt TEXT DEFAULT (datetime('now')),
                FOREIGN KEY(UserId) REFERENCES Users(UserId) ON DELETE CASCADE
            );
            CREATE TABLE IF NOT EXISTS OrderItems(
                OrderItemId INTEGER PRIMARY KEY AUTOINCREMENT,
                OrderId INTEGER NOT NULL,
                ProductId INTEGER NOT NULL,
                ProductName TEXT NOT NULL,
                Price REAL NOT NULL,
                Quantity INTEGER NOT NULL,
                FOREIGN KEY(OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
                FOREIGN KEY(ProductId) REFERENCES Products(ProductId)
            );
            INSERT INTO Products(ProductId,Name,Price)
            SELECT 101,'测试商品',9.99 WHERE NOT EXISTS(SELECT 1 FROM Products WHERE ProductId=101);
            INSERT INTO Products(ProductId,Name,Price)
            SELECT 102,'旗舰手机 X1',3999 WHERE NOT EXISTS(SELECT 1 FROM Products WHERE ProductId=102);
            INSERT INTO Products(ProductId,Name,Price)
            SELECT 103,'智能手表 S2',899 WHERE NOT EXISTS(SELECT 1 FROM Products WHERE ProductId=103);
            INSERT INTO Products(ProductId,Name,Price)
            SELECT 104,'轻薄笔记本 Pro',5999 WHERE NOT EXISTS(SELECT 1 FROM Products WHERE ProductId=104);
            INSERT INTO Products(ProductId,Name,Price)
            SELECT 105,'5G 智能手机 Pro Max',5299 WHERE NOT EXISTS(SELECT 1 FROM Products WHERE ProductId=105);
            INSERT INTO Products(ProductId,Name,Price)
            SELECT 106,'青春版手机 Lite',1999 WHERE NOT EXISTS(SELECT 1 FROM Products WHERE ProductId=106);
            INSERT INTO Products(ProductId,Name,Price)
            SELECT 107,'iPad Pro 12.9寸',6799 WHERE NOT EXISTS(SELECT 1 FROM Products WHERE ProductId=107);
            INSERT INTO Products(ProductId,Name,Price)
            SELECT 108,'安卓平板 Air',2499 WHERE NOT EXISTS(SELECT 1 FROM Products WHERE ProductId=108);
            INSERT INTO Products(ProductId,Name,Price)
            SELECT 109,'游戏本 RTX 4060',8999 WHERE NOT EXISTS(SELECT 1 FROM Products WHERE ProductId=109);
            INSERT INTO Products(ProductId,Name,Price)
            SELECT 110,'运动手环 Pro',399 WHERE NOT EXISTS(SELECT 1 FROM Products WHERE ProductId=110);
            INSERT INTO Products(ProductId,Name,Price)
            SELECT 111,'智能音箱 Echo',599 WHERE NOT EXISTS(SELECT 1 FROM Products WHERE ProductId=111);
            INSERT INTO Products(ProductId,Name,Price)
            SELECT 112,'智能门锁 Pro',1299 WHERE NOT EXISTS(SELECT 1 FROM Products WHERE ProductId=112);
            INSERT INTO Products(ProductId,Name,Price)
            SELECT 113,'智能摄像头 4K',399 WHERE NOT EXISTS(SELECT 1 FROM Products WHERE ProductId=113);
            INSERT INTO Products(ProductId,Name,Price)
            SELECT 114,'无线充电器',199 WHERE NOT EXISTS(SELECT 1 FROM Products WHERE ProductId=114);
            INSERT INTO Products(ProductId,Name,Price)
            SELECT 115,'蓝牙耳机 AirPods',1299 WHERE NOT EXISTS(SELECT 1 FROM Products WHERE ProductId=115);
            ";
            cmd.ExecuteNonQuery();
        }
    }
}