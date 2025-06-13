using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
            try
            {
                var response = await _authService.LoginAsync(loginDto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Login failed", details = ex.Message });
            }
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

        [Authorize(Roles = "User,Agent")]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email)?.Value;
            if (email == null)
            {
                return Unauthorized("User not authenticated.");
            }

            var user = await _authService.GetUserInfoAsync(email);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }
    }
}