using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace EcommerceApp.DAL
{
    public class CartDalSqlite
    {
        private readonly string _conn;
        public CartDalSqlite(string conn) => _conn = conn;

        public void AddItem(long userId,long productId,int qty)
        {
            using var con=new SqliteConnection(_conn);
            con.Open();
            using var cmd=con.CreateCommand();
            cmd.CommandText=@"INSERT INTO ShoppingCarts(UserId) SELECT @uid WHERE NOT EXISTS(SELECT 1 FROM ShoppingCarts WHERE UserId=@uid);
                              WITH cid AS(SELECT CartId FROM ShoppingCarts WHERE UserId=@uid)
                              INSERT INTO CartItems(CartId,ProductId,Quantity) VALUES((SELECT CartId FROM cid),@pid,@qty)
                              ON CONFLICT(CartId,ProductId) DO UPDATE SET Quantity=Quantity+@qty;";
            cmd.Parameters.AddWithValue("@uid",userId);
            cmd.Parameters.AddWithValue("@pid",productId);
            cmd.Parameters.AddWithValue("@qty",qty);
            cmd.ExecuteNonQuery();
        }

        public IEnumerable<CartItemDto> GetCart(long userId)
        {
            const string sql=@"SELECT p.ProductId,p.Name,p.Price,ci.Quantity
                               FROM ShoppingCarts sc
                               JOIN CartItems ci ON ci.CartId=sc.CartId
                               JOIN Products p ON p.ProductId=ci.ProductId
                               WHERE sc.UserId=@uid";
            using var con=new SqliteConnection(_conn);
            con.Open();
            using var cmd=con.CreateCommand();
            cmd.CommandText=sql;
            cmd.Parameters.AddWithValue("@uid",userId);
            using var rd=cmd.ExecuteReader();
            var list=new List<CartItemDto>();
            while(rd.Read())
            {
                list.Add(new CartItemDto(
                    rd.GetInt64(0),
                    rd.GetString(1),
                    rd.GetDecimal(2),
                    rd.GetInt32(3)));
            }
            return list;
        }

        public void RemoveItem(long userId,long productId)
        {
            const string sql=@"DELETE FROM CartItems WHERE CartId=(SELECT CartId FROM ShoppingCarts WHERE UserId=@uid) AND ProductId=@pid";
            using var con=new SqliteConnection(_conn);
            con.Open();
            using var cmd=con.CreateCommand();
            cmd.CommandText=sql;
            cmd.Parameters.AddWithValue("@uid",userId);
            cmd.Parameters.AddWithValue("@pid",productId);
            cmd.ExecuteNonQuery();
        }
    }


}
