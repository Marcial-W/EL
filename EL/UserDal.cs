using System;
using System.Data;
using System.Data.SqlClient;

namespace EcommerceApp.DAL
{
    /// <summary>
    /// 用户数据访问层，负责对 Users 表进行增查操作。
    /// </summary>
    public class UserDal
    {
        private readonly string _connectionString;

        /// <summary>
        /// 初始化数据访问层。
        /// </summary>
        /// <param name="connectionString">SQL Server 连接字符串。</param>
        public UserDal(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("连接字符串不能为空", nameof(connectionString));

            _connectionString = connectionString;
        }

        /// <summary>
        /// 注册新用户。
        /// </summary>
        /// <param name="email">邮箱（唯一）。</param>
        /// <param name="phone">手机号（可为空）。</param>
        /// <param name="passwordHash">已加密或哈希后的密码。</param>
        /// <param name="nickName">用户昵称。</param>
        /// <param name="role">角色：0=普通用户，1=管理员。</param>
        /// <returns>插入成功返回新用户的自增主键；失败返回 0。</returns>
        public long RegisterUser(string email, string? phone, string passwordHash, string nickName, byte role = 0)
        {
            const string sql = @"INSERT INTO Users (Email, Phone, PasswordHash, NickName, Role)
                                 VALUES (@Email, @Phone, @PasswordHash, @NickName, @Role);
                                 SELECT SCOPE_IDENTITY();";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 100).Value = email;
            cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 20).Value = (object?)phone ?? DBNull.Value;
            cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 256).Value = passwordHash;
            cmd.Parameters.Add("@NickName", SqlDbType.NVarChar, 50).Value = nickName;
            cmd.Parameters.Add("@Role", SqlDbType.TinyInt).Value = role;

            conn.Open();
            var result = cmd.ExecuteScalar();
            return result != null && long.TryParse(result.ToString(), out var newId) ? newId : 0;
        }

        /// <summary>
        /// 验证用户登录。
        /// </summary>
        /// <param name="identifier">邮箱或手机号。</param>
        /// <param name="passwordHash">已加密或哈希后的密码。</param>
        /// <returns>如果验证成功返回用户基本信息，否则返回 null。</returns>
        public UserDto? ValidateLogin(string identifier, string passwordHash)
        {
            const string sql = @"SELECT TOP 1 UserId, Email, Phone, NickName, Role, CreatedAt
                                 FROM Users
                                 WHERE (Email = @Identifier OR Phone = @Identifier)
                                   AND PasswordHash = @PasswordHash";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);

            cmd.Parameters.Add("@Identifier", SqlDbType.NVarChar, 100).Value = identifier;
            cmd.Parameters.Add("@PasswordHash", SqlDbType.NVarChar, 256).Value = passwordHash;

            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new UserDto
                {
                    UserId = reader.GetInt64("UserId"),
                    Email = reader.GetString("Email"),
                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString("Phone"),
                    NickName = reader.GetString("NickName"),
                    Role = reader.GetByte("Role"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                };
            }
            return null;
        }
    }

    /// <summary>
    /// 用户数据传输对象（与前端交互或业务层使用）。
    /// </summary>
    public class UserDto
    {
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string NickName { get; set; } = string.Empty;
        public byte Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // SqlDataReader 扩展方法，提升可读性
    internal static class DataReaderExtensions
    {
        public static long GetInt64(this SqlDataReader reader, string column)
            => reader.GetInt64(reader.GetOrdinal(column));
        public static string GetString(this SqlDataReader reader, string column)
            => reader.GetString(reader.GetOrdinal(column));
        public static byte GetByte(this SqlDataReader reader, string column)
            => reader.GetByte(reader.GetOrdinal(column));
        public static DateTime GetDateTime(this SqlDataReader reader, string column)
            => reader.GetDateTime(reader.GetOrdinal(column));
    }
}
