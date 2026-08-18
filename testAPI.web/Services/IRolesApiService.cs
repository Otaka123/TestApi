using testAPI.api.domain.DTOs.Role;

namespace testAPI.web.Services;

public interface IRolesApiService
{
    Task<ApiListResponse<RoleDTO>> GetAllAsync(string token, CancellationToken ct = default);
    Task<ApiSingleResponse<RoleDTO>> GetByIdAsync(int id, string token, CancellationToken ct = default);
    Task<ApiResponse> CreateAsync(CreateRoleDTO dto, string token, CancellationToken ct = default);
    Task<ApiResponse> UpdateAsync(int id, UpdateRoleDTO dto, string token, CancellationToken ct = default);
    Task<ApiResponse> DeleteAsync(int id, string token, CancellationToken ct = default);
}

// Generic API response models
public class ApiResponse
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class ApiListResponse<T> : ApiResponse
{
    public int Count { get; set; }
    public List<T> Data { get; set; } = new();
}

public class ApiSingleResponse<T> : ApiResponse
{
    public T? Data { get; set; }
}
