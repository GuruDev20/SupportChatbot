using SupportChatbot.API.DTOs;

namespace SupportChatbot.API.Interfaces
{
    public interface IChatService
    {
        public Task<ChatSessionResponseDto> StartChatAsync(ChatSessionCreateDto dto);
        public Task<List<ChatSessionResponseDto>> GetAllChatsAsync();
        public Task<ChatSessionResponseDto> GetChatByIdAsync(Guid chatId);
        public Task<MessageResponseDto> SendMessageAsync(MessageCreateDto dto);
        public Task<List<MessageResponseDto>> GetMessagesByChatIdAsync(Guid chatId);
        public Task<ChatSessionResponseDto?> EndChatAsync(Guid chatSessionId);
    }
}