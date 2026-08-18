using testAPI.api.domain.DTOs.User;

namespace testAPI.web.Services;

public interface IUsersApiService
{
    Task<ApiListResponse<UserResponseDto>> GetAllAsync(string token, CancellationToken ct = default);
    Task<ApiSingleResponse<UserResponseDto>> GetByIdAsync(int id, string token, CancellationToken ct = default);
    Task<ApiResponse> CreateAsync(CreateUserRequest dto, string token, CancellationToken ct = default);
    Task<ApiResponse> UpdateAsync(int id, EditUserDto dto, string token, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(int id, string token, CancellationToken ct = default);
    Task<ApiResponse> ToggleLockAsync(int id, string token, CancellationToken ct = default);
    Task<ApiResponse> ResetPasswordAsync(int id, string newPassword, string token, CancellationToken ct = default);
    Task<ApiListResponse<RoleOption>> GetRolesAsync(string token, CancellationToken ct = default);
}

public class RoleOption
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
