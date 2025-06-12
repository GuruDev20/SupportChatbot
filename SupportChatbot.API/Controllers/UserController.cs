using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportChatbot.API.DTOs;
using SupportChatbot.API.Interfaces;

namespace SupportChatbot.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            if (dto == null)
            {
                return BadRequest("User registration data cannot be null.");
            }

            try
            {
                var user = await _userService.RegisterUserAsync(dto);
                return Created("", user);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error registering user: {ex.Message}");
            }
        }

        [HttpDelete("{userId}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty.");
            }

            try
            {
                var result = await _userService.DeleteUserAsync(userId);
                if (result)
                {
                    return NoContent();
                }
                return NotFound($"User with ID {userId} not found.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error deleting user: {ex.Message}");
            }
        }

        [HttpPut("{userId}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(Guid userId, [FromBody] UpdateUserDto dto)
        {
            if (userId == Guid.Empty)
            {
                return BadRequest("User ID cannot be empty.");
            }

            if (dto == null)
            {
                return BadRequest("User update data cannot be null.");
            }

            try
            {
                var updatedUser = await _userService.UpdateUserAsync(userId, dto);
                if (updatedUser != null)
                {
                    return Ok(updatedUser);
                }
                return NotFound($"User with ID {userId} not found.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating user: {ex.Message}");
            }
        }
    }
}