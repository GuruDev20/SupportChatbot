using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportChatbot.API.Models
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [StringLength(100, ErrorMessage = "Chat Session ID must be a valid GUID.")]
        public Guid ChatSessionId { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Sender ID must be a valid GUID.")]
        public Guid SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsFile { get; set; } = false;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        [ForeignKey(nameof(ChatSessionId))]
        public ChatSession? ChatSession { get; set; }
        [ForeignKey(nameof(SenderId))]
        public User? Sender { get; set; }
    }
}