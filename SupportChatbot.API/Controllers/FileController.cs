using Microsoft.AspNetCore.Mvc;
using SupportChatbot.API.DTOs;
using SupportChatbot.API.Interfaces;

namespace SupportChatbot.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/files")]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;
        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("File name cannot be empty.");
            }

            try
            {
                var (content, contentType, name) = await _fileService.GetFileAsync(fileName);
                return File(content, contentType, name);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadDto dto)
        {
            if (dto == null || dto.File == null || dto.File.Length == 0)
            {
                return BadRequest("Invalid file upload data.");
            }

            try
            {
                var response = await _fileService.UploadFileAsync(dto);
                return Created(response.FileUrl, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}