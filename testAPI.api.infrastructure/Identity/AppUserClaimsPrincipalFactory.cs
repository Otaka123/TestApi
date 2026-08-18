using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace testAPI.api.infrastructure.Identity
{
    public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, AppRole>
    {
        public AppUserClaimsPrincipalFactory(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            IOptions<IdentityOptions> options)
            : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);

            if (!string.IsNullOrEmpty(user.FullName))
                identity.AddClaim(new Claim("FullName", user.FullName));

            if (user.UserTypeId.HasValue)
                identity.AddClaim(new Claim("UserTypeId", user.UserTypeId.Value.ToString()));

            if (user.AuthorityId.HasValue)
                identity.AddClaim(new Claim("AuthorityId", user.AuthorityId.Value.ToString()));

            return identity;
        }
    }
}
