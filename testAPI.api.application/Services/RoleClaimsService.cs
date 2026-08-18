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

        public RoleClaimsService(RoleManager<AppRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<ClaimsModel?> GetClaimsForRoleAsync(int roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) return null;

            var existingClaims = await _roleManager.GetClaimsAsync(role);

            return new ClaimsModel
            {
                RoleId = role.Id,
                VisitClaimsList = Build(ClaimStore.VisitClaimsList, existingClaims),
                HistoryClaimsList = Build(ClaimStore.HistoryClaimsList, existingClaims),
                CycleResultsClaimsList = Build(ClaimStore.CycleResultsClaimsList, existingClaims),
                RolesClaimsList = Build(ClaimStore.RolesClaimsList, existingClaims),
                UsersClaimsList = Build(ClaimStore.UsersClaimsList, existingClaims),
                MessagesClaimsList = Build(ClaimStore.MessagesClaimsList, existingClaims),
                CallClaimsList = Build(ClaimStore.CallClaimsList, existingClaims),
                WebsiteClaimsList = Build(ClaimStore.WebsiteClaimsList, existingClaims),
                AuthorityReplyClaimsList = Build(ClaimStore.AuthorityReplyClaimsList, existingClaims),
                AiTrainingClaimsList = Build(ClaimStore.AiTrainingClaimsList, existingClaims),
                CycleClaimsList = Build(ClaimStore.CycleClaimsList, existingClaims),
                ImprovementChance = Build(ClaimStore.ImprovementChance, existingClaims),
                SettingsClaimsList = Build(ClaimStore.SettingsClaimsList, existingClaims)
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
                model.VisitClaimsList, model.HistoryClaimsList, model.CycleResultsClaimsList,
                model.RolesClaimsList, model.UsersClaimsList, model.MessagesClaimsList,
                model.CallClaimsList, model.WebsiteClaimsList, model.ImprovementChance,
                model.AuthorityReplyClaimsList, model.AiTrainingClaimsList,
                model.CycleClaimsList, model.SettingsClaimsList
            };

            foreach (var group in allGroups)
            {
                foreach (var claimItem in group)
                {
                    if (claimItem.IsSelected)
                        await _roleManager.AddClaimAsync(role, new Claim(claimItem.ClaimType, claimItem.Label ?? ""));
                }
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
