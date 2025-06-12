using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportChatbot.API.DTOs;
using SupportChatbot.API.DTOs.Auth;
using SupportChatbot.API.Interfaces;

namespace SupportChatbot.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginDto)
        {
            if (loginDto == null || string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
            {
                return BadRequest("Invalid login request.");
            }

            var result = await _authService.LoginAsync(loginDto);
            if (result == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto refreshTokenDto)
        {
            if (refreshTokenDto == null || string.IsNullOrEmpty(refreshTokenDto.RefreshToken))
            {
                return BadRequest("Invalid refresh token request.");
            }

            var result = await _authService.RefreshTokenAsync(refreshTokenDto);
            if (result == null)
            {
                return Unauthorized("Invalid refresh token.");
            }

            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequestDto logoutDto)
        {
            if (logoutDto == null || string.IsNullOrEmpty(logoutDto.RefreshToken))
            {
                return BadRequest("Invalid logout request.");
            }
            await _authService.LogoutAsync(logoutDto.RefreshToken);
            return Ok("Logged out successfully.");
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userId == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var user = await _authService.GetUserInfoAsync(userId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }
    }
}