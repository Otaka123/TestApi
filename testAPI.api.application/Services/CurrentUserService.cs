using testAPI.api.application.ServiceInterfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace testAPI.api.application.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?
                    .User?
                    .FindFirst(ClaimTypes.NameIdentifier)?
                    .Value;

                return int.TryParse(userIdClaim, out var id) ? id : null;
            }
        }
    }
}
