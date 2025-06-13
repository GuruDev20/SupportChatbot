using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SupportChatbot.API.DTOs;
using SupportChatbot.API.Interfaces;

namespace SupportChatbot.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/chats")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }


        [HttpPost("start")]

        public async Task<IActionResult> StartConversation([FromBody] ChatSessionCreateDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid chat session data.");
            }

            var session = await _chatService.StartChatAsync(dto);
            if (session == null)
            {
                return StatusCode(500, "Failed to start chat session.");
            }
            var hubContext = HttpContext.RequestServices.GetRequiredService<IHubContext<ChatHub>>();
            await hubContext.Clients.Group("Agents").SendAsync("ReceiveNotification", new { sessionId = session.Id });
            return Created("", session);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllChats()
        {
            var chats = await _chatService.GetAllChatsAsync();
            if (chats == null || !chats.Any())
            {
                return NotFound("No chat sessions found.");
            }

            return Ok(chats);
        }

        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetChatById(Guid chatId)
        {
            if (chatId == Guid.Empty)
            {
                return BadRequest("Invalid chat ID.");
            }

            var chat = await _chatService.GetChatByIdAsync(chatId);
            if (chat == null)
            {
                return NotFound("Chat session not found.");
            }

            return Ok(chat);
        }

        [HttpGet("messages/{chatSessionId}")]
        public async Task<IActionResult> GetMessagesByChatId(Guid chatSessionId)
        {
            if (chatSessionId == Guid.Empty)
            {
                return BadRequest("Invalid chat session ID.");
            }

            var messages = await _chatService.GetMessagesByChatIdAsync(chatSessionId);
            if (messages == null || !messages.Any())
            {
                return NotFound("No messages found for this chat session.");
            }

            return Ok(messages);
        }

        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage([FromBody] MessageCreateDto dto)
        {
            if (dto == null || dto.ChatSessionId == Guid.Empty || string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest("Invalid message data.");
            }

            var message = await _chatService.SendMessageAsync(dto);
            if (message == null)
            {
                return StatusCode(500, "Failed to send message.");
            }

            return Created("", message);
        }

        [HttpPost("end/{chatId}")]
        public async Task<IActionResult> EndChat(Guid chatId)
        {
            if (chatId == Guid.Empty)
            {
                return BadRequest("Invalid chat session ID.");
            }

            var result = await _chatService.EndChatAsync(chatId);
            if (result == null)
            {
                return NotFound("Chat session not found or already ended.");
            }

            return Ok(result);
        }
    }
}