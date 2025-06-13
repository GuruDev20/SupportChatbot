using Microsoft.EntityFrameworkCore;
using SupportChatbot.API.Contexts;
using SupportChatbot.API.Interfaces;
using SupportChatbot.API.Models;

namespace SupportChatbot.API.Repositories
{
    public class MessageRepository : Repository<Guid, Message>, IMessageRepository
    {
        private readonly SupportChatbotContext _context;

        public MessageRepository(SupportChatbotContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Message>> GetMessagesByChatSessionIdAsync(Guid chatSessionId)
        {
            return await _context.Messages
                .Where(m => m.ChatSessionId == chatSessionId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }
    }
}
