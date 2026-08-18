using testAPI.api.domain.DTOs.Role;

namespace testAPI.api.application.ServiceInterfaces
{
    public interface IRoleService
    {
        Task<bool> CreateRoleAsync(CreateRoleDTO roleDto);
        Task<bool> UpdateRoleAsync(UpdateRoleDTO roleDto);
        Task<bool> SoftDeleteRoleAsync(int id);
        Task<bool> HardDeleteRoleAsync(int id);
        Task<bool> RestoreRoleAsync(int id);
        Task<List<RoleDTO>> GetAllRolesAsync();
        Task<List<RoleDTO>> GetAllRolesWithDeletedAsync();
        Task<RoleDTO?> GetRoleByIdAsync(int id);
        Task<RoleDTO?> GetDeletedRoleByIdAsync(int id);
        Task<bool> RoleNameExistsAsync(string name, int? excludeId = null);
        Task<byte[]> ExportRolesToExcelAsync();
    }
}
