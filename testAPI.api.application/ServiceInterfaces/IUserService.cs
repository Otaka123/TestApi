using testAPI.api.domain.DTOs.User;
using testAPI.api.domain.Entities;
using testAPI.api.infrastructure.Identity;

namespace testAPI.api.application.ServiceInterfaces
{
    public interface IUserService
    {
        Task<List<UserResponseDto>> GetAllAsync(bool? includeDeleted = true, CancellationToken cancellationToken = default);
        Task<List<UserResponseDto>> GetAllUsersNamesAsync(int? userType);
        Task<UserResponse> AddAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
        Task<UserResponse> EditAsync(EditUserDto request, CancellationToken cancellationToken = default);
        Task<bool> HardDeleteAsync(int id);
        Task<bool> SoftDeleteAsync(int id);
        Task<List<UserType>> GetUserTypes();
        Task<List<UserResponseDto>> GetUsersAsync(UsersFilterRequestDto filter);
        Task<UserResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(bool success, bool isLocked, string message)> ToggleUserLockAsync(long id);
        Task<byte[]> ExportUsersToExcelAsync(UsersFilterRequestDto filter);
        Task<UserType> GetCurrentUserTypeAsync();
        Task<AppUser> GetCurrentUserAsync();
        Task<int?> GetCurrentUserTypeIdAsync();
        Task<(bool success, bool is2faEnabled, string message)> ToggleTwoFactorAsync(int id);
        Task<string?> GetUserFullNameAsync(int userId);
        Task<string?> GetAuthorityNameByIdAsync(int authorityId);
    }
}
