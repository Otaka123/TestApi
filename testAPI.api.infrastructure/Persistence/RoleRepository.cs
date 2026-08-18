using testAPI.api.infrastructure.Data;
using testAPI.api.infrastructure.Identity;
using testAPI.api.infrastructure.Persistence.Interface;
using Microsoft.EntityFrameworkCore;
using static testAPI.api.infrastructure.Persistence.Repo;

namespace testAPI.api.infrastructure.Persistence
{
    public class RoleRepository : Repository<AppRole>, IRoleRepository
    {
        public RoleRepository(AppDbContext db) : base(db)
        {
        }

        public async Task<AppRole?> GetByNameAsync(string roleName)
        {
            return await dbSet.FirstOrDefaultAsync(r => r.Name == roleName);
        }

        public async Task<List<AppRole>> GetActiveRolesAsync()
        {
            return await dbSet.Where(r => !r.isDeleted).ToListAsync();
        }
    }
}
