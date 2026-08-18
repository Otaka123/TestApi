using testAPI.api.domain.DTOs.User;
using testAPI.api.domain.Entities;
using testAPI.api.infrastructure.Identity;
using static testAPI.api.infrastructure.Persistence.Interface.IRepo;

namespace testAPI.api.infrastructure.Persistence.Interface
{
    public interface IUserRepository : IRepo<AppUser>
    {
        Task<List<UserType>> GetUserTypes();
        Task<List<UserResponseDto>> GetUsersAsync(UsersFilterRequestDto filter);
        Task<UserResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<UserResponseDto>> GetAllAsync(bool? includeDeleted, CancellationToken cancellationToken = default);
        Task<List<UserResponseDto>> GetAllNamesWithIdsAsync(int? userType, CancellationToken cancellationToken = default);
    }
}
