using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace EcommerceApp.DAL
{
    public class OrderDalSqlite
    {
        private readonly string _conn;
        public OrderDalSqlite(string conn) => _conn = conn;

        // 创建订单
        public long CreateOrder(long userId, List<CartItemDto> cartItems)
        {
            if (cartItems == null || cartItems.Count == 0)
                return 0;

            using var con = new SqliteConnection(_conn);
            con.Open();
            using var trans = con.BeginTransaction();
            try
            {
                // 计算总金额
                decimal totalAmount = 0;
                foreach (var item in cartItems)
                {
                    totalAmount += item.Price * item.Quantity;
                }

                // 生成订单号
                var orderNumber = $"ORD{DateTime.Now:yyyyMMddHHmmss}{userId}";

                // 创建订单
                using var cmdOrder = con.CreateCommand();
                cmdOrder.Transaction = trans;
                cmdOrder.CommandText = @"INSERT INTO Orders(UserId, OrderNumber, TotalAmount, Status) 
                                        VALUES(@uid, @orderNo, @total, '待付款')
                                        RETURNING OrderId;";
                cmdOrder.Parameters.AddWithValue("@uid", userId);
                cmdOrder.Parameters.AddWithValue("@orderNo", orderNumber);
                cmdOrder.Parameters.AddWithValue("@total", totalAmount);
                var orderId = (long)(cmdOrder.ExecuteScalar() ?? 0);

                if (orderId == 0)
                {
                    trans.Rollback();
                    return 0;
                }

                // 创建订单项
                using var cmdItem = con.CreateCommand();
                cmdItem.Transaction = trans;
                cmdItem.CommandText = @"INSERT INTO OrderItems(OrderId, ProductId, ProductName, Price, Quantity) 
                                       VALUES(@oid, @pid, @name, @price, @qty);";
                
                foreach (var item in cartItems)
                {
                    cmdItem.Parameters.Clear();
                    cmdItem.Parameters.AddWithValue("@oid", orderId);
                    cmdItem.Parameters.AddWithValue("@pid", item.ProductId);
                    cmdItem.Parameters.AddWithValue("@name", item.Name);
                    cmdItem.Parameters.AddWithValue("@price", item.Price);
                    cmdItem.Parameters.AddWithValue("@qty", item.Quantity);
                    cmdItem.ExecuteNonQuery();
                }

                trans.Commit();
                return orderId;
            }
            catch
            {
                trans.Rollback();
                throw;
            }
        }

        // 获取用户的所有订单
        public IEnumerable<OrderDto> GetOrders(long userId)
        {
            const string sql = @"SELECT OrderId, OrderNumber, TotalAmount, Status, CreatedAt
                               FROM Orders
                               WHERE UserId = @uid
                               ORDER BY CreatedAt DESC";
            using var con = new SqliteConnection(_conn);
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@uid", userId);
            using var rd = cmd.ExecuteReader();
            var list = new List<OrderDto>();
            while (rd.Read())
            {
                list.Add(new OrderDto(
                    rd.GetInt64(0),
                    rd.GetString(1),
                    rd.GetDecimal(2),
                    rd.GetString(3),
                    rd.GetString(4)
                ));
            }
            return list;
        }

        // 获取订单详情（包含订单项）
        public OrderDetailDto? GetOrderDetail(long userId, long orderId)
        {
            const string sql = @"SELECT o.OrderId, o.OrderNumber, o.TotalAmount, o.Status, o.CreatedAt
                               FROM Orders o
                               WHERE o.OrderId = @oid AND o.UserId = @uid";
            using var con = new SqliteConnection(_conn);
            con.Open();
            using var cmd = con.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddWithValue("@oid", orderId);
            cmd.Parameters.AddWithValue("@uid", userId);
            using var rd = cmd.ExecuteReader();
            
            if (!rd.Read())
                return null;

            var order = new OrderDetailDto(
                rd.GetInt64(0),
                rd.GetString(1),
                rd.GetDecimal(2),
                rd.GetString(3),
                rd.GetString(4),
                new List<OrderItemDto>()
            );

            // 获取订单项
            const string itemSql = @"SELECT ProductId, ProductName, Price, Quantity
                                   FROM OrderItems
                                   WHERE OrderId = @oid";
            using var cmdItem = con.CreateCommand();
            cmdItem.CommandText = itemSql;
            cmdItem.Parameters.AddWithValue("@oid", orderId);
            using var rdItem = cmdItem.ExecuteReader();
            while (rdItem.Read())
            {
                order.Items.Add(new OrderItemDto(
                    rdItem.GetInt64(0),
                    rdItem.GetString(1),
                    rdItem.GetDecimal(2),
                    rdItem.GetInt32(3)
                ));
            }

            return order;
        }
    }

    public record OrderDto(long OrderId, string OrderNumber, decimal TotalAmount, string Status, string CreatedAt);
    public record OrderItemDto(long ProductId, string ProductName, decimal Price, int Quantity);
    public record OrderDetailDto(long OrderId, string OrderNumber, decimal TotalAmount, string Status, string CreatedAt, List<OrderItemDto> Items);
}
