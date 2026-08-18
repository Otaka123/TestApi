using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using testAPI.api.domain.DTOs.Role;

namespace testAPI.web.Services;

public class RolesApiService : IRolesApiService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public RolesApiService(HttpClient http) => _http = http;

    private void SetAuth(string token) =>
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public async Task<ApiListResponse<RoleDTO>> GetAllAsync(string token, CancellationToken ct = default)
    {
        SetAuth(token);
        var resp = await _http.GetAsync("api/Roles", ct);
        
        if (!resp.IsSuccessStatusCode)
        {
            return new ApiListResponse<RoleDTO> 
            { 
                Succeeded = false, 
                Message = resp.StatusCode == System.Net.HttpStatusCode.Unauthorized 
                    ? "غير مصرح بالدخول" 
                    : "فشل الاتصال" 
            };
        }
        
        return await resp.Content.ReadFromJsonAsync<ApiListResponse<RoleDTO>>(_json, ct)
               ?? new ApiListResponse<RoleDTO> { Succeeded = false, Message = "فشل الاتصال" };
    }

    public async Task<ApiSingleResponse<RoleDTO>> GetByIdAsync(int id, string token, CancellationToken ct = default)
    {
        SetAuth(token);
        var resp = await _http.GetAsync($"api/Roles/{id}", ct);
        
        if (!resp.IsSuccessStatusCode)
        {
            return new ApiSingleResponse<RoleDTO> 
            { 
                Succeeded = false, 
                Message = resp.StatusCode == System.Net.HttpStatusCode.Unauthorized 
                    ? "غير مصرح بالدخول" 
                    : "فشل الاتصال" 
            };
        }
        
        return await resp.Content.ReadFromJsonAsync<ApiSingleResponse<RoleDTO>>(_json, ct)
               ?? new ApiSingleResponse<RoleDTO> { Succeeded = false, Message = "فشل الاتصال" };
    }

    public async Task<ApiResponse> CreateAsync(CreateRoleDTO dto, string token, CancellationToken ct = default)
    {
        SetAuth(token);
        var resp = await _http.PostAsJsonAsync("api/Roles", dto, ct);
        
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

    public async Task<ApiResponse> UpdateAsync(int id, UpdateRoleDTO dto, string token, CancellationToken ct = default)
    {
        SetAuth(token);
        var resp = await _http.PutAsJsonAsync($"api/Roles/{id}", dto, ct);
        
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

    public async Task<ApiResponse> DeleteAsync(int id, string token, CancellationToken ct = default)
    {
        SetAuth(token);
        var resp = await _http.DeleteAsync($"api/Roles/{id}", ct);
        
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
