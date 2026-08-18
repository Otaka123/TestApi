using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using testAPI.api.domain.DTOs.Auth;

namespace testAPI.web.Services;

public class AuthApiService : IAuthApiService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<AuthResultDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<AuthResultDto>(_jsonOptions, cancellationToken);
            
            if (result != null)
                return result;

            return new AuthResultDto
            {
                Succeeded = false,
                Message = "فشل في قراءة الاستجابة من الخادم"
            };
        }
        catch (Exception ex)
        {
            return new AuthResultDto
            {
                Succeeded = false,
                Message = $"تعذر الاتصال بالخادم: {ex.Message}"
            };
        }
    }

    public async Task<AuthResultDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/Auth/register", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<AuthResultDto>(_jsonOptions, cancellationToken);

            if (result != null)
                return result;

            return new AuthResultDto
            {
                Succeeded = false,
                Message = "فشل في قراءة الاستجابة من الخادم"
            };
        }
        catch (Exception ex)
        {
            return new AuthResultDto
            {
                Succeeded = false,
                Message = $"تعذر الاتصال بالخادم: {ex.Message}"
            };
        }
    }

    public async Task<AuthResultDto> LogoutAsync(string? token, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/Auth/logout");
            
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResultDto>(_jsonOptions, cancellationToken);
                return result ?? new AuthResultDto { Succeeded = true, Message = "تم تسجيل الخروج بنجاح" };
            }

            return new AuthResultDto { Succeeded = true, Message = "تم تسجيل الخروج بنجاح" };
        }
        catch (Exception ex)
        {
            return new AuthResultDto
            {
                Succeeded = false,
                Message = $"خطأ أثناء تسجيل الخروج: {ex.Message}"
            };
        }
    }

    public async Task<AuthResultDto> GetCurrentUserAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/Auth/me");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AuthResultDto>(_jsonOptions, cancellationToken);
                return result ?? new AuthResultDto { Succeeded = false, Message = "فشل تحليل الاستجابة" };
            }

            return new AuthResultDto { Succeeded = false, Message = "فشل في جلب بيانات المستخدم" };
        }
        catch (Exception ex)
        {
            return new AuthResultDto
            {
                Succeeded = false,
                Message = $"خطأ أثناء جلب بيانات المستخدم: {ex.Message}"
            };
        }
    }
}
