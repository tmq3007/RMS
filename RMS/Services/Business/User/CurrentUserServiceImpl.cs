using RMS.Shared.Interfaces;
using System.Security.Claims;

namespace RMS.Services.Business.User
{
    public class CurrentUserServiceImpl : ICurrentuserService
    {
        public CurrentUserServiceImpl() { }
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUserServiceImpl(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?
                .FindFirstValue(ClaimTypes.NameIdentifier);
            return userId;
        }
    }
}
