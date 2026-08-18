using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace testAPI.web.Services;

public class RoleClaimsApiService : IRoleClaimsApiService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public RoleClaimsApiService(HttpClient http) => _http = http;

    private void SetAuth(string token) =>
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public async Task<RoleClaimsResponse> GetClaimsForRoleAsync(int roleId, string token, CancellationToken ct = default)
    {
        SetAuth(token);
        var resp = await _http.GetAsync($"api/roles/{roleId}/roleclaims", ct);
        
        if (!resp.IsSuccessStatusCode)
        {
            return new RoleClaimsResponse 
            { 
                Succeeded = false, 
                Message = resp.StatusCode == System.Net.HttpStatusCode.Unauthorized 
                    ? "غير مصرح بالدخول" 
                    : "فشل الاتصال" 
            };
        }
        
        return await resp.Content.ReadFromJsonAsync<RoleClaimsResponse>(_json, ct)
               ?? new RoleClaimsResponse { Succeeded = false, Message = "فشل الاتصال" };
    }

    public async Task<ApiResponse> UpdateClaimsAsync(int roleId, JsonElement model, string token, CancellationToken ct = default)
    {
        SetAuth(token);
        var content = new StringContent(model.GetRawText(), Encoding.UTF8, "application/json");
        var resp = await _http.PutAsync($"api/roles/{roleId}/roleclaims", content, ct);
        
        if (!resp.IsSuccessStatusCode)
        {
            return new ApiResponse 
            { 
                Succeeded = false, 
                Message = resp.StatusCode == System.Net.HttpStatusCode.Unauthorized 
                    ? "غير مصرح بالدخول" 
                    : "فشل الاتصال" 
            };
        }
        
        return await resp.Content.ReadFromJsonAsync<ApiResponse>(_json, ct)
               ?? new ApiResponse { Succeeded = false, Message = "فشل الاتصال" };
    }
}
