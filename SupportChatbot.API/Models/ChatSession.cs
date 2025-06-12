using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportChatbot.API.Models
{
    public class ChatSession
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [StringLength(100, ErrorMessage = "User ID must be a valid GUID.")]
        public Guid UserId { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Agent ID must be a valid GUID.")]
        public Guid AgentId { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
        [ForeignKey(nameof(AgentId))]
        public User? Agent { get; set; }
        public List<Message>? Messages { get; set; }
    }
}