using AutoMapper;
using Moq;
using SupportChatbot.API.DTOs;
using SupportChatbot.API.Interfaces;
using SupportChatbot.API.Models;
using SupportChatbot.API.Services;
using Xunit;

namespace SupportChatbot.Tests.Services
{
    public class ChatServiceTests
    {
        private readonly Mock<IRepository<Guid, ChatSession>> _chatSessionRepoMock;
        private readonly Mock<IRepository<Guid, User>> _userRepoMock;
        private readonly Mock<IRepository<Guid, Message>> _messageRepoMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly ChatService _chatService;

        public ChatServiceTests()
        {
            _chatSessionRepoMock = new Mock<IRepository<Guid, ChatSession>>();
            _userRepoMock = new Mock<IRepository<Guid, User>>();
            _mapperMock = new Mock<IMapper>();
            _messageRepoMock = new Mock<IRepository<Guid, Message>>();

            _chatService = new ChatService(
                _chatSessionRepoMock.Object,
                _userRepoMock.Object,
                _mapperMock.Object,
                _messageRepoMock.Object
            );
        }

        [Fact]
        public async Task GetAllChatsAsync_ReturnsChatSessionResponseDtos()
        {
            var sessions = new List<ChatSession> { new ChatSession { Id = Guid.NewGuid() } };
            var sessionDtos = new List<ChatSessionResponseDto> { new ChatSessionResponseDto { Id = sessions[0].Id } };

            _chatSessionRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(sessions);
            _mapperMock.Setup(m => m.Map<List<ChatSessionResponseDto>>(sessions)).Returns(sessionDtos);

            var result = await _chatService.GetAllChatsAsync();

            Assert.Single(result);
            Assert.Equal(sessionDtos[0].Id, result[0].Id);
        }

        [Fact]
        public async Task GetChatByIdAsync_ExistingId_ReturnsChatSessionResponseDto()
        {
            var sessionId = Guid.NewGuid();
            var session = new ChatSession { Id = sessionId };
            var sessionDto = new ChatSessionResponseDto { Id = sessionId };

            _chatSessionRepoMock.Setup(repo => repo.GetByIdAsync(sessionId)).ReturnsAsync(session);
            _mapperMock.Setup(m => m.Map<ChatSessionResponseDto>(session)).Returns(sessionDto);

            var result = await _chatService.GetChatByIdAsync(sessionId);

            Assert.Equal(sessionId, result.Id);
        }

        [Fact]
        public async Task GetMessagesByChatIdAsync_ReturnsMessageDtos()
        {
            var chatId = Guid.NewGuid();
            var messages = new List<Message>
            {
                new Message { ChatSessionId = chatId, Content = "Hi" }
            };
            var messageDtos = new List<MessageResponseDto>
            {
                new MessageResponseDto { Content = "Hi" }
            };

            _messageRepoMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(messages);
            _mapperMock.Setup(m => m.Map<List<MessageResponseDto>>(It.IsAny<List<Message>>())).Returns(messageDtos);

            var result = await _chatService.GetMessagesByChatIdAsync(chatId);

            Assert.Single(result);
            Assert.Equal("Hi", result[0].Content);
        }

        [Fact]
        public async Task SendMessageAsync_AddsMessage_ReturnsMessageResponseDto()
        {
            var dto = new MessageCreateDto
            {
                ChatSessionId = Guid.NewGuid(),
                SenderId = Guid.NewGuid(),
                Content = "Hello"
            };

            var message = new Message
            {
                ChatSessionId = dto.ChatSessionId,
                SenderId = dto.SenderId,
                Content = dto.Content
            };

            var messageResponse = new MessageResponseDto { Content = "Hello" };

            _messageRepoMock.Setup(repo => repo.AddAsync(It.IsAny<Message>()))
                            .ReturnsAsync(message);
            _mapperMock.Setup(m => m.Map<MessageResponseDto>(It.IsAny<Message>()))
                       .Returns(messageResponse);

            var result = await _chatService.SendMessageAsync(dto);

            Assert.Equal("Hello", result.Content);
        }

        [Fact]
        public async Task StartChatAsync_Success_ReturnsChatSessionResponseDto()
        {
            var userId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var users = new List<User>
            {
                new User { Id = userId, Role = "User", IsActive = true },
                new User { Id = agentId, Role = "Agent", IsActive = true }
            };

            var session = new ChatSession
            {
                UserId = userId,
                AgentId = agentId
            };

            var sessionDto = new ChatSessionResponseDto { UserId = userId, AgentId = agentId };

            _userRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(users.First(u => u.Id == userId));
            _chatSessionRepoMock.Setup(r => r.AddAsync(It.IsAny<ChatSession>())).ReturnsAsync(session);
            _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
            _mapperMock.Setup(m => m.Map<ChatSessionResponseDto>(It.IsAny<ChatSession>())).Returns(sessionDto);

            var result = await _chatService.StartChatAsync(new ChatSessionCreateDto { UserId = userId });

            Assert.Equal(userId, result.UserId);
            Assert.Equal(agentId, result.AgentId);
        }

        [Fact]
        public async Task EndChatAsync_EndsSessionAndReturnsDto()
        {
            var sessionId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var agentId = Guid.NewGuid();

            var session = new ChatSession
            {
                Id = sessionId,
                UserId = userId,
                AgentId = agentId,
                Status = "Active"
            };

            var user = new User { Id = userId, IsActive = false };
            var agent = new User { Id = agentId, IsActive = false };

            var sessionDto = new ChatSessionResponseDto { Id = sessionId };

            _chatSessionRepoMock.Setup(r => r.GetByIdAsync(sessionId)).ReturnsAsync(session);
            _userRepoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
            _userRepoMock.Setup(r => r.GetByIdAsync(agentId)).ReturnsAsync(agent);
            _chatSessionRepoMock.Setup(r => r.UpdateAsync(It.IsAny<ChatSession>())).ReturnsAsync(session);
            _userRepoMock.Setup(r => r.UpdateAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
            _mapperMock.Setup(m => m.Map<ChatSessionResponseDto>(It.IsAny<ChatSession>())).Returns(sessionDto);

            var result = await _chatService.EndChatAsync(sessionId);

            Assert.NotNull(result);
            Assert.Equal(sessionId, result.Id);
        }
    }
}
