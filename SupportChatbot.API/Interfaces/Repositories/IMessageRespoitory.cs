using SupportChatbot.API.Models;

namespace SupportChatbot.API.Interfaces
{
    public interface IMessageRepository : IRepository<Guid, Message>
    {
        Task<IEnumerable<Message>> GetMessagesByChatSessionIdAsync(Guid chatSessionId);
    }
}
