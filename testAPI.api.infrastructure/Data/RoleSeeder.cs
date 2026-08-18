using testAPI.api.domain.Entities;
using testAPI.api.infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace testAPI.api.infrastructure.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            // 1. Seed UserTypes if empty
            if (!await context.UserTypes.AnyAsync())
            {
                var userTypes = new List<UserType>
                {
                    new UserType { TypeName = "أدمن" },
                    new UserType { TypeName = "موظف" },
                    new UserType { TypeName = "جهة" }
                };
                await context.UserTypes.AddRangeAsync(userTypes);
                await context.SaveChangesAsync();
            }

            // 2. Seed Roles
            string[] roles = { "superadmin", "admin", "user" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new AppRole { Name = roleName, isDeleted = false });
                }
            }

            // 3. Assign All Claims to superadmin
            var superAdminRole = await roleManager.FindByNameAsync("superadmin");
            if (superAdminRole != null)
            {
                var existingClaims = await roleManager.GetClaimsAsync(superAdminRole);
                var existingClaimTypes = existingClaims.Select(c => c.Type).ToHashSet();

                var allClaims = GetAllSystemClaims();
                foreach (var claim in allClaims)
                {
                    if (!existingClaimTypes.Contains(claim.Type))
                    {
                        await roleManager.AddClaimAsync(superAdminRole, claim);
                    }
                }
            }

            // 4. Seed Default SuperAdmin User
            var superAdminEmail = "superadmin@testAPI.com";
            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail)
                ?? await userManager.FindByNameAsync("superadmin");

            if (superAdminUser == null)
            {
                var adminUserType = await context.UserTypes.FirstOrDefaultAsync(ut => ut.TypeName == "أدمن")
                    ?? await context.UserTypes.FirstOrDefaultAsync();

                superAdminUser = new AppUser
                {
                    FullName = "Super Admin",
                    UserName = "superadmin",
                    Email = superAdminEmail,
                    EmailConfirmed = true,
                    PhoneNumber = "0500000000",
                    UserTypeId = adminUserType?.Id,
                    isDeleted = false
                };

                var result = await userManager.CreateAsync(superAdminUser, "SuperAdmin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(superAdminUser, "superadmin");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(superAdminUser, "superadmin"))
                {
                    await userManager.AddToRoleAsync(superAdminUser, "superadmin");
                }
            }
        }

        private static List<Claim> GetAllSystemClaims()
        {
            var list = new List<Claim>();
            list.AddRange(ClaimStore.VisitClaimsList);
            list.AddRange(ClaimStore.HistoryClaimsList);
            list.AddRange(ClaimStore.CycleResultsClaimsList);
            list.AddRange(ClaimStore.RolesClaimsList);
            list.AddRange(ClaimStore.UsersClaimsList);
            list.AddRange(ClaimStore.MessagesClaimsList);
            list.AddRange(ClaimStore.CallClaimsList);
            list.AddRange(ClaimStore.CycleClaimsList);
            list.AddRange(ClaimStore.SettingsClaimsList);
            list.AddRange(ClaimStore.WebsiteClaimsList);
            list.AddRange(ClaimStore.ImprovementChance);
            list.AddRange(ClaimStore.AuthorityReplyClaimsList);
            list.AddRange(ClaimStore.AiTrainingClaimsList);
            return list;
        }
    }
}
