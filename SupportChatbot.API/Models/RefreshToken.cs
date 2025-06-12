using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportChatbot.API.Models
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [StringLength(100, ErrorMessage = "User ID must be a valid GUID.")]
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}