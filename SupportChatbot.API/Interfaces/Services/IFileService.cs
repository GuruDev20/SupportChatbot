using SupportChatbot.API.DTOs;

namespace SupportChatbot.API.Interfaces
{
    public interface IFileService
    {
        public Task<FileResponseDto> UploadFileAsync(FileUploadDto dto);
        public Task<(byte[] FileContent,string ContentType,string FileName)> GetFileAsync(string fileName);
    }
}