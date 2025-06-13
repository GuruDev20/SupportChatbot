namespace SupportChatbot.API.DTOs
{
    public class MessageCreateDto
    {
        public Guid ChatSessionId { get; set; }
        public Guid SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsFile { get; set; } = false;
    }
}