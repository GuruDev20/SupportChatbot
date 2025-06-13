namespace SupportChatbot.API.DTOs
{
    public class FileResponseDto
    {
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
}