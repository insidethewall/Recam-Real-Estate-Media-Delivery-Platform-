using AutoMapper;
using RecamSystemApi.DTOs;
using RecamSystemApi.Models;

namespace RecamSystemApi.Mapping;

    public class MappingProfile: Profile
    {
    public MappingProfile()
    {
        IMappingExpression<RegisterRequestDto, User> UserMapper = CreateMap<RegisterRequestDto, User>();
            UserMapper.ForAllMembers(opt => opt.Ignore());
          UserMapper.ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
          .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
          .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
        }
    }
