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

            // 5. Seed Admin Role with All Claims
            var adminRole = await roleManager.FindByNameAsync("admin");
            if (adminRole != null)
            {
                var existingAdminClaims = await roleManager.GetClaimsAsync(adminRole);
                var existingAdminClaimTypes = existingAdminClaims.Select(c => c.Type).ToHashSet();

                var allClaims = GetAllSystemClaims();
                foreach (var claim in allClaims)
                {
                    if (!existingAdminClaimTypes.Contains(claim.Type))
                    {
                        await roleManager.AddClaimAsync(adminRole, claim);
                    }
                }
            }

            // 6. Seed Default Admin User
            var adminEmail = "admin@testAPI.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail)
                ?? await userManager.FindByNameAsync("admin");

            if (adminUser == null)
            {
                var adminUserType = await context.UserTypes.FirstOrDefaultAsync(ut => ut.TypeName == "أدمن")
                    ?? await context.UserTypes.FirstOrDefaultAsync();

                adminUser = new AppUser
                {
                    FullName = "Admin User",
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PhoneNumber = "0500000001",
                    UserTypeId = adminUserType?.Id,
                    isDeleted = false
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "admin");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, "admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "admin");
                }
            }
        }

        private static List<Claim> GetAllSystemClaims()
        {
            var list = new List<Claim>();
            list.AddRange(ClaimStore.HomeClaimsList);
            list.AddRange(ClaimStore.RolesClaimsList);
            list.AddRange(ClaimStore.UsersClaimsList);
            return list;
        }
    }
}
