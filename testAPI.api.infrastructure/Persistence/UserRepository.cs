using testAPI.api.domain.DTOs.User;
using testAPI.api.domain.Entities;
using testAPI.api.infrastructure.Data;
using testAPI.api.infrastructure.Identity;
using testAPI.api.infrastructure.Persistence.Interface;
using Microsoft.EntityFrameworkCore;
using static testAPI.api.infrastructure.Persistence.Repo;

namespace testAPI.api.infrastructure.Persistence
{
    public class UserRepository : Repository<AppUser>, IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<List<UserType>> GetUserTypes()
        {
            return await _context.UserTypes.ToListAsync();
        }

        public async Task<List<UserResponseDto>> GetUsersAsync(UsersFilterRequestDto filter)
        {
            var query = from u in _context.Users
                        join ur in _context.UserRoles on u.Id equals ur.UserId into userRoles
                        from ur in userRoles.DefaultIfEmpty()
                        join r in _context.Roles on ur.RoleId equals r.Id into roles
                        from r in roles.DefaultIfEmpty()
                        select new
                        {
                            User = u,
                            RoleId = r != null ? r.Id : (int?)null,
                            RoleName = r != null ? r.Name : null
                        };

            if (filter.UserId.HasValue)
                query = query.Where(x => x.User.Id == filter.UserId.Value);

            if (filter.RoleId.HasValue)
                query = query.Where(x => x.RoleId == filter.RoleId.Value);

            if (filter.UserTypeId.HasValue)
                query = query.Where(x => x.User.UserTypeId == filter.UserTypeId.Value);

            return await query.OrderByDescending(x => x.User.Id).Select(x => new UserResponseDto
            {
                Id = x.User.Id,
                FullName = x.User.FullName,
                UserName = x.User.UserName!,
                Email = x.User.Email!,
                PhoneNumber = x.User.PhoneNumber != null && x.User.PhoneNumber.StartsWith("971")
                    ? x.User.PhoneNumber.Substring(3)
                    : x.User.PhoneNumber!,
                RoleName = x.RoleName,
                UserType = x.User.UserType!.TypeName,
                IsLocked = x.User.LockoutEnd.HasValue && x.User.LockoutEnd > DateTimeOffset.Now,
                DeveloperCode = x.User.DeveloperCode,
                IsTwoFactorEnabled = x.User.TwoFactorEnabled
            }).ToListAsync();
        }

        public async Task<UserResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await (
                from u in _context.Users
                join ur in _context.UserRoles on u.Id equals ur.UserId into userRoles
                from ur in userRoles.DefaultIfEmpty()
                join r in _context.Roles on ur.RoleId equals r.Id into roles
                from r in roles.DefaultIfEmpty()
                where u.Id == id
                select new UserResponseDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email!,
                    UserName = u.UserName!,
                    PhoneNumber = u.PhoneNumber != null && u.PhoneNumber.StartsWith("971")
                        ? u.PhoneNumber.Substring(3)
                        : u.PhoneNumber!,
                    IsDeleted = u.isDeleted,
                    RoleId = r.Id,
                    RoleName = r.Name,
                    UserTypeId = u.UserTypeId,
                    UserType = u.UserType!.TypeName,
                    DeveloperCode = u.DeveloperCode,
                    AuthorityId = u.AuthorityId
                }
            ).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<UserResponseDto>> GetAllAsync(bool? includeDeleted = true, CancellationToken cancellationToken = default)
        {
            return await (
                from u in _context.Users
                join ur in _context.UserRoles on u.Id equals ur.UserId into userRoles
                from ur in userRoles.DefaultIfEmpty()
                join r in _context.Roles on ur.RoleId equals r.Id into roles
                from r in roles.DefaultIfEmpty()
                where includeDeleted == true || !u.isDeleted
                select new UserResponseDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email!,
                    UserName = u.UserName!,
                    PhoneNumber = u.PhoneNumber != null && u.PhoneNumber.StartsWith("971")
                        ? u.PhoneNumber.Substring(3)
                        : u.PhoneNumber!,
                    IsDeleted = u.isDeleted,
                    RoleId = r.Id,
                    RoleName = r.Name,
                    UserTypeId = u.UserTypeId,
                    UserType = u.UserType!.TypeName,
                    DeveloperCode = u.DeveloperCode
                }
            ).ToListAsync(cancellationToken);
        }

        public async Task<List<UserResponseDto>> GetAllNamesWithIdsAsync(int? userType, CancellationToken cancellationToken = default)
        {
            if (userType is null || userType == 0)
            {
                return await (
                    from u in _context.Users.Include(x => x.UserType)
                    where !u.isDeleted
                    select new UserResponseDto
                    {
                        Id = u.Id,
                        FullName = string.IsNullOrWhiteSpace(u.FullName) ? u.UserName! : u.FullName,
                        UserTypeId = u.UserTypeId,
                        UserType = u.UserType!.TypeName
                    }
                ).ToListAsync(cancellationToken);
            }
            else
            {
                return await (
                    from u in _context.Users.Include(x => x.UserType)
                    where !u.isDeleted && u.UserTypeId == userType.Value
                    select new UserResponseDto
                    {
                        Id = u.Id,
                        FullName = string.IsNullOrWhiteSpace(u.FullName) ? u.UserName! : u.FullName,
                        UserTypeId = u.UserTypeId,
                        UserType = u.UserType!.TypeName
                    }
                ).ToListAsync(cancellationToken);
            }
        }
    }
}
