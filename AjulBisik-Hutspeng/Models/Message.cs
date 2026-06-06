using System;

namespace AjulBisik_Hutspeng.Models
{
    public class Message
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Content { get; set; }
        public bool IsAnonymous { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // New properties for Inbox/Reply/Reaction
        public bool IsRead { get; set; }
        public string ReplyContent { get; set; }
        public string Reaction { get; set; }

        public User Sender { get; set; } // For UI
    }
}
