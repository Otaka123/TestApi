using testAPI.api.infrastructure.Identity;
using static testAPI.api.infrastructure.Persistence.Interface.IRepo;

namespace testAPI.api.infrastructure.Persistence.Interface
{
    public interface IRoleRepository : IRepo<AppRole>
    {
        Task<AppRole?> GetByNameAsync(string roleName);
        Task<List<AppRole>> GetActiveRolesAsync();
    }
}
