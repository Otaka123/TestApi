using testAPI.api.infrastructure.Identity;

namespace testAPI.api.application.ServiceInterfaces
{
    public interface IRoleClaimsService
    {
        Task<ClaimsModel?> GetClaimsForRoleAsync(int roleId);
        Task<bool> UpdateRoleClaimsAsync(int roleId, ClaimsModel model);
    }
}
