using AutoMapper;
using Microsoft.AspNetCore.Identity;
using RMS.Data.Entities;
using RMS.DTOs.Requests.User;
using RMS.DTOs.Responses.User;
using RMS.Shared.Interfaces;

namespace RMS.Services.Business.User
{
    public class UserServiceImpl : IUserService
    {
        private readonly ITokenService _tokenService;
        private readonly ICurrentuserService _currentuserService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly ILogger<UserServiceImpl> _logger;
        public Task<UserResponse> RegisterAsync(UserRegisterRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UserResponse> LoginAsync(UserLoginRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request)
        {
            throw new NotImplementedException();
        }
        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<UserResponse> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<CurrentUserResponse> GetCurrentUserAsync()
        {
            throw new NotImplementedException();
        }
       
        public Task<CurrentUserResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            throw new NotImplementedException();
        }


        public Task<RevokeRefreshTokenResponse> RevokeRefreshTokenAsync(RefreshTokenRequest request)
        {
            throw new NotImplementedException();
        }

    }
}
