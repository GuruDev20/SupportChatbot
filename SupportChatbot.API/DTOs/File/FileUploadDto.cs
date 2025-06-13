namespace SupportChatbot.API.DTOs
{
    public class FileUploadDto
    {
        public Guid ChatSessionId { get; set; }
        public Guid UploaderId { get; set; }
        public IFormFile File{ get; set; } = null!; 
    }
}