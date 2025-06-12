using Microsoft.EntityFrameworkCore;
using SupportChatbot.API.Contexts;
using SupportChatbot.API.Interfaces;
using SupportChatbot.API.Models;

namespace SupportChatbot.API.Repositories
{
    public class ChatRepository : Repository<Guid, ChatSession>, IChatRepository
    {
        private readonly SupportChatbotContext _context;
        public ChatRepository(SupportChatbotContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ChatSession>> GetByAgentIdAsync(Guid agentId)
        {
            if (agentId == Guid.Empty)
            {
                throw new ArgumentException("Agent ID cannot be empty", nameof(agentId));
            }
            return await _context.ChatSessions
                .Where(cs => cs.AgentId == agentId)
                .Include(cs => cs.Messages)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatSession>> GetByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            }
            return await _context.ChatSessions
                .Where(cs => cs.UserId == userId)
                .Include(cs => cs.Messages)
                .ToListAsync();
        }
    }
}