using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportChatbot.API.Models
{
    public class FileUpload
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [StringLength(100, ErrorMessage = "Chat Session ID must be a valid GUID.")]
        public Guid ChatSessionId { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "Upploader ID must be a valid GUID.")]
        public Guid UploaderId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public byte[]? FileContent { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        [ForeignKey(nameof(ChatSessionId))]
        public ChatSession? ChatSession { get; set; }
        [ForeignKey(nameof(UploaderId))]
        public User? Uploader { get; set; }
    }
}