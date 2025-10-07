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

    CreateMap<UpdateListingCaseDto, ListingCase>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.UserId, opt => opt.Ignore())
        .ForMember(dest => dest.User, opt => opt.Ignore())
        .ForMember(dest => dest.AgentListingCases, opt => opt.Ignore())
        .ForMember(dest => dest.MediaAssets, opt => opt.Ignore())
        .ForMember(dest => dest.CaseContacts, opt => opt.Ignore());

      IMappingExpression<MediaAssetDto, MediaAsset> MediaAssetDtoMapper = CreateMap<MediaAssetDto, MediaAsset>()
      .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
      .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
              
        }
    }
