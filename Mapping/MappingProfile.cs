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
    UserMapper
    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

    IMappingExpression<Agent, RegisterRequestDto> AgentDtoMapper = CreateMap<Agent, RegisterRequestDto>();
    
    IMappingExpression<ListingCaseDto, ListingCase> ListingCaseDtoMapper = CreateMap<ListingCaseDto, ListingCase>()
    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
            
      }
    }
