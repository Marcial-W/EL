using System;

namespace EcommerceApp.DAL
{
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

    /// <summary>
    /// 购物车项数据传输对象。
    /// </summary>
    public record CartItemDto(long ProductId, string Name, decimal Price, int Quantity);
}

