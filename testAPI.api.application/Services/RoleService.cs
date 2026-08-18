using testAPI.api.application.ServiceInterfaces;
using testAPI.api.domain.DTOs.Role;
using testAPI.api.infrastructure.Identity;
using testAPI.api.infrastructure.Persistence.Interface;
using Microsoft.AspNetCore.Identity;

namespace testAPI.api.application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IExcelExportService _excelExportService;
        private readonly UserManager<AppUser> _userManager;

        public RoleService(IRoleRepository roleRepository, RoleManager<AppRole> roleManager,
            IExcelExportService excelExportService, UserManager<AppUser> userManager)
        {
            _roleRepository = roleRepository;
            _roleManager = roleManager;
            _excelExportService = excelExportService;
            _userManager = userManager;
        }

        public async Task<bool> CreateRoleAsync(CreateRoleDTO roleDto)
        {
            try
            {
                if (await RoleNameExistsAsync(roleDto.Name))
                    return false;

                var role = new AppRole { Name = roleDto.Name };
                var result = await _roleManager.CreateAsync(role);
                return result.Succeeded;
            }
            catch { return false; }
        }

        public async Task<bool> UpdateRoleAsync(UpdateRoleDTO roleDto)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(roleDto.Id.ToString());
                if (role == null || role.isDeleted)
                    return false;

                if (await RoleNameExistsAsync(roleDto.Name, roleDto.Id))
                    return false;

                role.Name = roleDto.Name;
                var result = await _roleManager.UpdateAsync(role);
                return result.Succeeded;
            }
            catch { return false; }
        }

        public async Task<bool> SoftDeleteRoleAsync(int id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null || role.isDeleted)
                    return false;

                if (await RoleHasUsersAsync(id))
                    return false;

                role.isDeleted = true;
                var result = await _roleManager.UpdateAsync(role);
                return result.Succeeded;
            }
            catch { return false; }
        }

        public async Task<bool> HardDeleteRoleAsync(int id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null)
                    return false;

                if (await RoleHasUsersAsync(id))
                    return false;

                var result = await _roleManager.DeleteAsync(role);
                return result.Succeeded;
            }
            catch { return false; }
        }

        public async Task<bool> RestoreRoleAsync(int id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null || !role.isDeleted)
                    return false;

                role.isDeleted = false;
                var result = await _roleManager.UpdateAsync(role);
                return result.Succeeded;
            }
            catch { return false; }
        }

        public async Task<List<RoleDTO>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync(r => !r.isDeleted);
            return roles.OrderByDescending(r => r.Id).Select(r => new RoleDTO { Id = r.Id, Name = r.Name! }).ToList();
        }

        public async Task<List<RoleDTO>> GetAllRolesWithDeletedAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(r => new RoleDTO { Id = r.Id, Name = r.Name!, IsDeleted = r.isDeleted }).ToList();
        }

        public async Task<RoleDTO?> GetRoleByIdAsync(int id)
        {
            var role = await _roleRepository.GetAsync(r => r.Id == id && !r.isDeleted);
            if (role == null) return null;
            return new RoleDTO { Id = role.Id, Name = role.Name! };
        }

        public async Task<RoleDTO?> GetDeletedRoleByIdAsync(int id)
        {
            var role = await _roleRepository.GetAsync(r => r.Id == id && r.isDeleted);
            if (role == null) return null;
            return new RoleDTO { Id = role.Id, Name = role.Name!, IsDeleted = role.isDeleted };
        }

        public async Task<bool> RoleNameExistsAsync(string name, int? excludeId = null)
        {
            if (excludeId.HasValue)
                return await _roleRepository.AnyAsync(r => r.Name == name && r.Id != excludeId.Value && !r.isDeleted);

            return await _roleRepository.AnyAsync(r => r.Name == name && !r.isDeleted);
        }

        public async Task<byte[]> ExportRolesToExcelAsync()
        {
            var roles = await GetAllRolesAsync();
            int counter = 1;
            var exportData = roles.Select(r => new { الرقم = counter++, اسم_الدور = r.Name }).ToList();
            var headers = new List<string> { "#", "اسم الدور" };
            return _excelExportService.ExportToExcel(exportData, headers, "الأدوار");
        }

        private async Task<bool> RoleHasUsersAsync(int roleId)
        {
            var roleName = (await _roleManager.FindByIdAsync(roleId.ToString()))?.Name ?? "";
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            return usersInRole.Any();
        }
    }
}
