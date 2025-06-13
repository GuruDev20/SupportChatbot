namespace SupportChatbot.API.DTOs
{
    public class ChatSessionResponseDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid AgentId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
    }
}