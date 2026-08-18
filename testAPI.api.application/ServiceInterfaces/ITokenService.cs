using testAPI.api.domain.DTOs.Auth;
using testAPI.api.infrastructure.Identity;
using System.Security.Claims;

namespace testAPI.api.application.ServiceInterfaces
{
    public interface ITokenService
    {
        Task<TokenResponseDto> CreateTokenAsync(AppUser user, CancellationToken cancellationToken = default);
        Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<RefreshToken?> GetActiveRefreshTokenByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task StoreRefreshTokenAsync(int userId, string refreshToken, DateTime expiresOn, CancellationToken cancellationToken = default);
        Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
        Task RevokeAllUserRefreshTokensAsync(int userId, CancellationToken cancellationToken = default);
        Task<AuthResultDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
