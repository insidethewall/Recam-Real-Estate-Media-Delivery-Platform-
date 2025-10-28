using AutoMapper;
using RecamSystemApi.DTOs;
using RecamSystemApi.Models;

namespace RecamSystemApi.Mapping;

public class MappingProfile : Profile
{
  public MappingProfile()
  {
    IMappingExpression<RegisterRequestDto, User> UserMapper = CreateMap<RegisterRequestDto, User>();
    UserMapper.ForAllMembers(opt => opt.Ignore());
    UserMapper
    .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

    IMappingExpression<IUserProfileDto, Agent> AgentMapper = CreateMap<IUserProfileDto, Agent>()
    .ForMember(dest => dest.AgentFirstName, opt => opt.MapFrom(src => src.FirstName))
    .ForMember(dest => dest.AgentLastName, opt => opt.MapFrom(src => src.LastName));

    IMappingExpression<IUserProfileDto, Photographer> PhotographerMapper = CreateMap<IUserProfileDto, Photographer>()
    .ForMember(dest => dest.PhotographerFirstName, opt => opt.MapFrom(src => src.FirstName))
    .ForMember(dest => dest.PhotographerLastName, opt => opt.MapFrom(src => src.LastName));
    

    IMappingExpression<Agent, RegisterRequestDto> AgentDtoMapper = CreateMap<Agent, RegisterRequestDto>();

    IMappingExpression<ListingCaseDto, ListingCase> ListingCaseDtoMapper = CreateMap<ListingCaseDto, ListingCase>()
    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
    .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

    IMappingExpression<UpdateListingCaseDto, ListingCase> UpdateListingCaseDtoMapper = CreateMap<UpdateListingCaseDto, ListingCase>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
        .ForMember(dest => dest.UserId, opt => opt.Ignore())
        .ForMember(dest => dest.User, opt => opt.Ignore())
        .ForMember(dest => dest.AgentListingCases, opt => opt.Ignore())
        .ForMember(dest => dest.MediaAssets, opt => opt.Ignore())
        .ForMember(dest => dest.CaseContacts, opt => opt.Ignore())
        .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(_ => false));

    IMappingExpression<ListingCase, ListingCaseWithNavDto> ListingCaseNavDtoMapper =  CreateMap<ListingCase, ListingCaseWithNavDto>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
            .ForMember(dest => dest.AgentListingCases, opt => opt.MapFrom(src => src.AgentListingCases))
            .ForMember(dest => dest.MediaAssets, opt => opt.MapFrom(src => src.MediaAssets))
            .ForMember(dest => dest.CaseContacts, opt => opt.MapFrom(src => src.CaseContacts));
    CreateMap<User, RegisterRequestDto>();      // maps ListingCase.User → RegisterRequestDto
  

    IMappingExpression<MediaAssetDto, MediaAsset> MediaAssetDtoMapper = CreateMap<MediaAssetDto, MediaAsset>()
    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid().ToString()))
    .ForMember(dest => dest.UploadedAt, opt => opt.MapFrom(src => DateTime.UtcNow));


    IMappingExpression<MediaAsset, MediaAssetDto> MediaAssetMapper = CreateMap<MediaAsset, MediaAssetDto>();
  
  }
        
    }
