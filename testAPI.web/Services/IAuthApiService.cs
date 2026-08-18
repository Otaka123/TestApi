using testAPI.api.domain.DTOs.Auth;

namespace testAPI.web.Services;

public interface IAuthApiService
{
    Task<AuthResultDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResultDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResultDto> LogoutAsync(string? token, CancellationToken cancellationToken = default);
}
