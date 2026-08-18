using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using testAPI.api.domain.DTOs.User;

namespace testAPI.web.Services;

public class UsersApiService : IUsersApiService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public UsersApiService(HttpClient http) => _http = http;

    private void SetAuth(string token) =>
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    public async Task<ApiListResponse<UserResponseDto>> GetAllAsync(string token, CancellationToken ct = default)
    {
        SetAuth(token);
        var resp = await _http.GetAsync("api/Users", ct);
        
        if (!resp.IsSuccessStatusCode)
        {
            return new ApiListResponse<UserResponseDto> 
            { 
                Succeeded = false, 
                Message = resp.StatusCode == System.Net.HttpStatusCode.Unauthorized 
                    ? "غير مصرح بالدخول" 
                    : "فشل الاتصال" 
            };
        }
        
        return await resp.Content.ReadFromJsonAsync<ApiListResponse<UserResponseDto>>(_json, ct)
               ?? new ApiListResponse<UserResponseDto> { Succeeded = false, Message = "فشل الاتصال" };
    }

    public async Task<ApiSingleResponse<UserResponseDto>> GetByIdAsync(int id, string token, CancellationToken ct = default)
    {
        SetAuth(token);
        var resp = await _http.GetAsync($"api/Users/{id}", ct);
        
        if (!resp.IsSuccessStatusCode)
        {
            return new ApiSingleResponse<UserResponseDto> 
            { 
                Succeeded = false, 
                Message = resp.StatusCode == System.Net.HttpStatusCode.Unauthorized 
                    ? "غير مصرح بالدخول" 
                    : "فشل الاتصال" 
            };
        }
        
        return await resp.Content.ReadFromJsonAsync<ApiSingleResponse<UserResponseDto>>(_json, ct)
               ?? new ApiSingleResponse<UserResponseDto> { Succeeded = false, Message = "فشل الاتصال" };
    }

    public async Task<ApiResponse> CreateAsync(CreateUserRequest dto, string token, CancellationToken ct = default)
    {
        SetAuth(token);
        var resp = await _http.PostAsJsonAsync("api/Users", dto, ct);
        
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

    public async Task<ApiResponse> UpdateAsync(int id, EditUserDto dto, string token, CancellationToken ct = default)
    {
        SetAuth(token);
        var resp = await _http.PutAsJsonAsync($"api/Users/{id}", dto, ct);
        
        try
        {
            var body = await resp.Content.ReadFromJsonAsync<ApiResponse>(_json, ct);
            if (body != null && !string.IsNullOrEmpty(body.Message))
                return body;
        }
        catch
        {
            // If response is not JSON
        }
        
        if (!resp.IsSuccessStatusCode)
        {
            return new ApiResponse 
            { 
                Succeeded = false, 
                Message = resp.StatusCode == System.Net.HttpStatusCode.Unauthorized 
                    ? "غير مصرح بالدخول" 
                    : $"فشل الاتصال ({resp.StatusCode})" 
            };
        }
        
        return new ApiResponse { Succeeded = false, Message = "فشل الاتصال" };
    }

    public async Task<ApiResponse> DeleteAsync(int id, string token, CancellationToken ct = default)
    {
        SetAuth(token);
        var resp = await _http.DeleteAsync($"api/Users/{id}", ct);
        
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

    public async Task<ApiResponse> ToggleLockAsync(int id, string token, CancellationToken ct = default)
    {
        SetAuth(token);
        var resp = await _http.PostAsync($"api/Users/{id}/toggle-lock", null, ct);
        
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

    public async Task<ApiResponse> ResetPasswordAsync(int id, string newPassword, string token, CancellationToken ct = default)
    {
        SetAuth(token);
        var resp = await _http.PostAsJsonAsync($"api/Users/{id}/reset-password",
            new { NewPassword = newPassword, ConfirmPassword = newPassword }, ct);
        
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

    public async Task<ApiListResponse<RoleOption>> GetRolesAsync(string token, CancellationToken ct = default)
    {
        SetAuth(token);
        var resp = await _http.GetAsync("api/Auth/roles", ct);
        
        if (!resp.IsSuccessStatusCode)
        {
            return new ApiListResponse<RoleOption> 
            { 
                Succeeded = false, 
                Message = resp.StatusCode == System.Net.HttpStatusCode.Unauthorized 
                    ? "غير مصرح بالدخول" 
                    : "فشل الاتصال" 
            };
        }
        
        return await resp.Content.ReadFromJsonAsync<ApiListResponse<RoleOption>>(_json, ct)
               ?? new ApiListResponse<RoleOption> { Succeeded = false, Message = "فشل الاتصال" };
    }
}
