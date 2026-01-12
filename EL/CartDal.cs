using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace EcommerceApp.DAL
{
    public class CartDal
    {
        private readonly string _conn;
        public CartDal(string conn) => _conn = conn;

        public void AddItem(long userId, long productId, int qty)
        {
            const string sql = @"IF NOT EXISTS(SELECT 1 FROM ShoppingCarts WHERE UserId=@uid)
                                    INSERT INTO ShoppingCarts(UserId) VALUES(@uid);
                                  DECLARE @cartId BIGINT = (SELECT CartId FROM ShoppingCarts WHERE UserId=@uid);
                                  IF EXISTS(SELECT 1 FROM CartItems WHERE CartId=@cartId AND ProductId=@pid)
                                        UPDATE CartItems SET Quantity = Quantity + @qty WHERE CartId=@cartId AND ProductId=@pid;
                                  ELSE
                                        INSERT INTO CartItems(CartId,ProductId,Quantity) VALUES(@cartId,@pid,@qty);";
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@uid", SqlDbType.BigInt).Value = userId;
            cmd.Parameters.Add("@pid", SqlDbType.BigInt).Value = productId;
            cmd.Parameters.Add("@qty", SqlDbType.Int).Value = qty;
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public IEnumerable<CartItemDto> GetCart(long userId)
        {
            const string sql = @"SELECT ci.ProductId, p.Name, p.Price, ci.Quantity
                                 FROM ShoppingCarts sc
                                 JOIN CartItems ci ON sc.CartId = ci.CartId
                                 JOIN Products p ON p.ProductId = ci.ProductId
                                 WHERE sc.UserId = @uid";
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@uid", SqlDbType.BigInt).Value = userId;
            conn.Open();
            using var rd = cmd.ExecuteReader();
            var list = new List<CartItemDto>();
            while (rd.Read())
            {
                list.Add(new CartItemDto(
                    rd.GetInt64("ProductId"),
                    rd.GetString("Name"),
                    rd.GetDecimal("Price"),
                    rd.GetInt32("Quantity")));
            }
            return list;
        }

        public void RemoveItem(long userId, long productId)
        {
            const string sql = @"DELETE ci FROM CartItems ci
                                 JOIN ShoppingCarts sc ON sc.CartId = ci.CartId
                                 WHERE sc.UserId=@uid AND ci.ProductId=@pid";
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@uid", SqlDbType.BigInt).Value = userId;
            cmd.Parameters.Add("@pid", SqlDbType.BigInt).Value = productId;
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    public record CartItemDto(long ProductId, string Name, decimal Price, int Quantity);
}

