using System.Text.Json;

namespace testAPI.web.Services;

public interface IRoleClaimsApiService
{
    Task<RoleClaimsResponse> GetClaimsForRoleAsync(int roleId, string token, CancellationToken ct = default);
    Task<ApiResponse> UpdateClaimsAsync(int roleId, JsonElement model, string token, CancellationToken ct = default);
}

public class RoleClaimsResponse : ApiResponse
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public JsonElement? RoleClaims { get; set; }
    public JsonElement? AllClaimCategories { get; set; }
    public List<string>? AllClaimTypes { get; set; }
}
