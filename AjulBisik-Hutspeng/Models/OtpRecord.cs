using System;

namespace AjulBisik_Hutspeng.Models
{
    public class OtpRecord
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string OtpCode { get; set; }
        public string Type { get; set; }
        public DateTime ExpiryTime { get; set; }
        public int ResendCount { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
