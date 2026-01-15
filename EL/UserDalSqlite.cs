using System;
using System.Data;
using Microsoft.Data.Sqlite;

namespace EcommerceApp.DAL
{
    public class UserDalSqlite
    {
        private readonly string _conn;
        public UserDalSqlite(string conn) => _conn = conn;

        public long RegisterUser(string email,string? phone,string pwdHash,string nickName,byte role=0){
            const string sql="INSERT INTO Users(Email,Phone,PasswordHash,NickName,Role) VALUES(@e,@p,@h,@n,@r);SELECT last_insert_rowid();";
            using var con=new SqliteConnection(_conn);
            con.Open();
            using var cmd=con.CreateCommand();
            cmd.CommandText=sql;
            cmd.Parameters.AddWithValue("@e",email);
            cmd.Parameters.AddWithValue("@p",(object?)phone??DBNull.Value);
            cmd.Parameters.AddWithValue("@h",pwdHash);
            cmd.Parameters.AddWithValue("@n",nickName);
            cmd.Parameters.AddWithValue("@r",role);
            return (long)(cmd.ExecuteScalar()??0);
        }
        public bool UserExists(string identifier){
            const string sql="SELECT 1 FROM Users WHERE Email=@id OR Phone=@id LIMIT 1";
            using var con=new SqliteConnection(_conn);
            con.Open();
            using var cmd=con.CreateCommand();
            cmd.CommandText=sql;
            cmd.Parameters.AddWithValue("@id",identifier);
            using var rd=cmd.ExecuteReader();
            return rd.Read();
        }
        public UserDto? ValidateLogin(string id,string pwdHash){
            const string sql="SELECT UserId,Email,Phone,NickName,Role,CreatedAt FROM Users WHERE (Email=@id OR Phone=@id) AND PasswordHash=@h LIMIT 1";
            using var con=new SqliteConnection(_conn);
            con.Open();
            using var cmd=con.CreateCommand();
            cmd.CommandText=sql;
            cmd.Parameters.AddWithValue("@id",id);
            cmd.Parameters.AddWithValue("@h",pwdHash);
            using var rd=cmd.ExecuteReader();
            if(rd.Read())
                return new UserDto{
                    UserId=rd.GetInt64(0),
                    Email=rd.GetString(1),
                    Phone=rd.IsDBNull(2)?null:rd.GetString(2),
                    NickName=rd.GetString(3),
                    Role=(byte)rd.GetInt64(4),
                    CreatedAt=rd.GetDateTime(5)
                };
            return null;
        }
    }
}

