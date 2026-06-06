using System;

namespace AjulBisik_Hutspeng.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public byte[] ProfilePhotoData { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
