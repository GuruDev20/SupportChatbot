namespace SupportChatbot.API.DTOs
{
    public class MessageResponseDto
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsFile { get; set; } = false;
        public DateTime SentAt { get; set; }
    }
}