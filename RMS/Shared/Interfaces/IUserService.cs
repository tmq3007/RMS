using RMS.DTOs.Requests.User;
using RMS.DTOs.Responses.User;

namespace RMS.Shared.Interfaces
{
    public interface IUserService
    {
        Task<UserResponse> RegisterAsync(UserRegisterRequest request);
        Task<CurrentUserResponse> GetCurrentUserAsync();
        Task<UserResponse> GetByIdAsync(Guid id);
        Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request);
        Task DeleteAsync(Guid id);
        Task<RevokeRefreshTokenResponse> RevokeRefreshTokenAsync(RefreshTokenRequest request);
        Task<CurrentUserResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<UserResponse> LoginAsync(UserLoginRequest request);

    }
}
