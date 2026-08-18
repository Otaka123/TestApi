using testAPI.api.application.ServiceInterfaces;
using testAPI.api.domain.Entities;
using testAPI.api.infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace testAPI.api.application.Services
{
    public class RoleClaimsService : IRoleClaimsService
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RoleClaimsService(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<ClaimsModel?> GetClaimsForRoleAsync(int roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) return null;

            var existingClaims = await _roleManager.GetClaimsAsync(role);

            return new ClaimsModel
            {
                RoleId = role.Id,
                HomeClaimsList = Build(ClaimStore.HomeClaimsList, existingClaims),
                RolesClaimsList = Build(ClaimStore.RolesClaimsList, existingClaims),
                UsersClaimsList = Build(ClaimStore.UsersClaimsList, existingClaims)
            };
        }

        public async Task<bool> UpdateRoleClaimsAsync(int roleId, ClaimsModel model)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) return false;

            var oldClaims = await _roleManager.GetClaimsAsync(role);
            foreach (var claim in oldClaims)
                await _roleManager.RemoveClaimAsync(role, claim);

            var allGroups = new[]
            {
                model.HomeClaimsList,
                model.RolesClaimsList,
                model.UsersClaimsList
            };

            foreach (var group in allGroups)
            {
                foreach (var claimItem in group)
                {
                    if (claimItem.IsSelected)
                        await _roleManager.AddClaimAsync(role, new Claim(claimItem.ClaimType, claimItem.Label ?? ""));
                }
            }

            // ✅ تحديث SecurityStamp لجميع المستخدمين في هذا الدور
            // هذا سيجبر المستخدمين على تحديث الـ Claims عند الطلب التالي
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name ?? "");
            foreach (var user in usersInRole)
            {
                await _userManager.UpdateSecurityStampAsync(user);
            }

            return true;
        }

        private List<ClaimSelection> Build(List<Claim> storeList, IList<Claim> existingClaims)
        {
            return storeList.Select(c => new ClaimSelection
            {
                ClaimType = c.Type,
                Label = c.Value,
                IsSelected = existingClaims.Any(ec => ec.Type == c.Type)
            }).ToList();
        }
    }
}
