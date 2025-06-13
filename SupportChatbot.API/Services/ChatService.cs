using AutoMapper;
using SupportChatbot.API.DTOs;
using SupportChatbot.API.Interfaces;
using SupportChatbot.API.Models;

namespace SupportChatbot.API.Services
{
    public class ChatService : IChatService
    {
        private readonly IRepository<Guid, ChatSession> _chatSessionRepository;
        private readonly IRepository<Guid, User> _userRepository;
        private readonly IMapper _mapper;
        private readonly IRepository<Guid, Message> _messageRepository;
        public ChatService(IRepository<Guid, ChatSession> chatSessionRepository, IRepository<Guid, User> userRepository, IMapper mapper, IRepository<Guid, Message> messageRepository)
        {
            _chatSessionRepository = chatSessionRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _messageRepository = messageRepository;
        }

        public async Task<List<ChatSessionResponseDto>> GetAllChatsAsync()
        {
            var chats = await _chatSessionRepository.GetAllAsync();
            return _mapper.Map<List<ChatSessionResponseDto>>(chats);
        }

        public async Task<ChatSessionResponseDto> GetChatByIdAsync(Guid chatId)
        {
            var chat = await _chatSessionRepository.GetByIdAsync(chatId);
            if (chat == null)
            {
                throw new KeyNotFoundException("Chat session not found.");
            }
            return _mapper.Map<ChatSessionResponseDto>(chat);
        }

        public async Task<List<MessageResponseDto>> GetMessagesByChatIdAsync(Guid chatId)
        {
            var messages = await _messageRepository.GetAllAsync();
            var chatMessages = messages.Where(m => m.ChatSessionId == chatId).ToList();
            if (chatMessages == null || !chatMessages.Any())
            {
                throw new KeyNotFoundException("No messages found for this chat session.");
            }
            return _mapper.Map<List<MessageResponseDto>>(chatMessages);
        }

        public async Task<MessageResponseDto> SendMessageAsync(MessageCreateDto dto)
        {
            var message = new Message
            {
                ChatSessionId = dto.ChatSessionId,
                SenderId = dto.SenderId,
                Content = dto.Content,
                IsFile = dto.IsFile,
            };
            await _messageRepository.AddAsync(message);
            return _mapper.Map<MessageResponseDto>(message);
        }

        public async Task<ChatSessionResponseDto> StartChatAsync(ChatSessionCreateDto dto)
        {
            var agents = await _userRepository.GetAllAsync();
            var availableAgent = agents.FirstOrDefault(u => u.Role == "Agent" && u.IsActive);
            if (availableAgent == null)
            {
                throw new InvalidOperationException("No available agents to start a chat.");
            }
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null || !user.IsActive)
            {
                throw new InvalidOperationException("User not found or already in a session.");
            }
            var session = new ChatSession
            {
                UserId = dto.UserId,
                AgentId = availableAgent.Id,
            };
            await _chatSessionRepository.AddAsync(session);
            user.IsActive = false;
            availableAgent.IsActive = false;
            await _userRepository.UpdateAsync(user);
            await _userRepository.UpdateAsync(availableAgent);
            return _mapper.Map<ChatSessionResponseDto>(session);
        }

        public async Task<ChatSessionResponseDto?> EndChatAsync(Guid chatSessionId)
        {
            var session = await _chatSessionRepository.GetByIdAsync(chatSessionId);
            if (session == null || session.Status == "Ended")
            {
                return null;
            }

            session.Status = "Ended";
            session.EndedAt = DateTime.UtcNow;

            var user = await _userRepository.GetByIdAsync(session.UserId);
            var agent = await _userRepository.GetByIdAsync(session.AgentId);

            if (user != null) user.IsActive = true;
            if (agent != null) agent.IsActive = true;

            await _chatSessionRepository.UpdateAsync(session);
            if (user != null) await _userRepository.UpdateAsync(user);
            if (agent != null) await _userRepository.UpdateAsync(agent);

            return _mapper.Map<ChatSessionResponseDto>(session);
        }
    }
}