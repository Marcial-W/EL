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
            INSERT INTO Products(ProductId,Name,Price)
            SELECT 101,'测试商品',9.99 WHERE NOT EXISTS(SELECT 1 FROM Products WHERE ProductId=101);
            ";
            cmd.ExecuteNonQuery();
        }
    }
}