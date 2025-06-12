using SupportChatbot.API.Models;

namespace SupportChatbot.API.Interfaces
{
    public interface IChatRepository : IRepository<Guid, ChatSession>
    {
        public Task<IEnumerable<ChatSession>> GetByUserIdAsync(Guid userId);
        public Task<IEnumerable<ChatSession>> GetByAgentIdAsync(Guid agentId);
    }
}