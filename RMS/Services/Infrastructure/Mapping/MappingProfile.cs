using AutoMapper;
using RMS.Data.Entities;
using RMS.DTOs.Requests.User;
using RMS.DTOs.Responses.User;

namespace RMS.Services.Infrastructure.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // CreateMap<Source, Destination>();
            // Example:
            // CreateMap<UpdateUserRequest, User>();
            // CreateMap<User, CurrentUserResponse>();
            // CreateMap<RefreshTokenRequest, TokenRequest>();
            // CreateMap<TokenResponse, RevokeRefreshTokenResponse>();

            CreateMap<ApplicationUser, UserResponse>();
            CreateMap<ApplicationUser, CurrentUserResponse>();
            CreateMap<UserRegisterRequest, ApplicationUser>();

        }
    }
}
