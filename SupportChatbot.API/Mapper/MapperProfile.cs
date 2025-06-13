using AutoMapper;
using SupportChatbot.API.DTOs;
using SupportChatbot.API.Models;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<ChatSession, ChatSessionResponseDto>();
        CreateMap<User, UserResponseDto>();
        CreateMap<Message, MessageResponseDto>();
    }
}
