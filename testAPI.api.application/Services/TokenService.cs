using testAPI.api.application.ServiceInterfaces;
using testAPI.api.domain.DTOs.Auth;
using testAPI.api.infrastructure.Data;
using testAPI.api.infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace testAPI.api.application.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly AppDbContext _context;

        public TokenService(
            IConfiguration configuration,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            AppDbContext context)
        {
            _configuration = configuration;
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public async Task<TokenResponseDto> CreateTokenAsync(AppUser user, CancellationToken cancellationToken = default)
        {
            var accessTokenExpireAt = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:AccessTokenExpirationInMinutes"] ?? "30"));
            var refreshTokenExpireAt = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpirationInDays"] ?? "7"));

            var accessToken = await GenerateAccessTokenAsync(user, accessTokenExpireAt, cancellationToken);
            var refreshToken = GenerateRefreshToken();

            await StoreRefreshTokenAsync(user.Id, refreshToken, refreshTokenExpireAt, cancellationToken);

            var profile = await GetUserProfileAsync(user, cancellationToken);

            return new TokenResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpireAt = accessTokenExpireAt,
                RefreshTokenExpireAt = refreshTokenExpireAt,
                TokenType = "Bearer",
                User = profile
            };
        }

        private async Task<string> GenerateAccessTokenAsync(AppUser user, DateTime expires, CancellationToken cancellationToken = default)
        {
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured"));

            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new System.Security.Claims.Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new System.Security.Claims.Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new System.Security.Claims.Claim(JwtRegisteredClaimNames.Iat,
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            if (!string.IsNullOrEmpty(user.FullName))
                claims.Add(new System.Security.Claims.Claim("FullName", user.FullName));

            if (user.AuthorityId.HasValue)
                claims.Add(new System.Security.Claims.Claim("AuthorityId", user.AuthorityId.Value.ToString()));

            if (user.UserTypeId.HasValue)
                claims.Add(new System.Security.Claims.Claim("UserTypeId", user.UserTypeId.Value.ToString()));

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new System.Security.Claims.Claim(ClaimTypes.Role, role));

                var appRole = await _roleManager.FindByNameAsync(role);
                if (appRole != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(appRole);
                    foreach (var roleClaim in roleClaims)
                    {
                        if (!claims.Any(c => c.Type == roleClaim.Type && c.Value == roleClaim.Value))
                            claims.Add(roleClaim);
                    }
                }
            }

            var userClaims = await _userManager.GetClaimsAsync(user);
            foreach (var userClaim in userClaims)
            {
                if (!claims.Any(c => c.Type == userClaim.Type && c.Value == userClaim.Value))
                    claims.Add(userClaim);
            }

            var symmetricKey = new SymmetricSecurityKey(key);
            var signingCredentials = new SigningCredentials(symmetricKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task StoreRefreshTokenAsync(int userId, string refreshToken, DateTime expiresOn, CancellationToken cancellationToken = default)
        {
            var token = new RefreshToken
            {
                UserId = userId,
                Token = refreshToken,
                ExpiresOn = expiresOn,
                CreatedOn = DateTime.UtcNow
            };

            await _context.RefreshTokens.AddAsync(token, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        }

        public async Task<RefreshToken?> GetActiveRefreshTokenByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt =>
                    rt.UserId == userId &&
                    rt.RevokedOn == null &&
                    rt.ExpiresOn >= DateTime.UtcNow,
                    cancellationToken);
        }

        public async Task RevokeRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);

            if (refreshToken != null)
            {
                refreshToken.RevokedOn = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RevokeAllUserRefreshTokensAsync(int userId, CancellationToken cancellationToken = default)
        {
            var userTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedOn == null)
                .ToListAsync(cancellationToken);

            foreach (var token in userTokens)
                token.RevokedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured"))),
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<AuthResultDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
        {
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null)
                return new AuthResultDto { Succeeded = false, Message = "Access Token غير صالح" };

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return new AuthResultDto { Succeeded = false, Message = "Access Token غير صالح - لم يتم العثور على المستخدم" };

            var storedRefreshToken = await GetRefreshTokenAsync(request.RefreshToken, cancellationToken);
            if (storedRefreshToken == null || storedRefreshToken.UserId != userId || storedRefreshToken.RevokedOn != null)
                return new AuthResultDto { Succeeded = false, Message = "Refresh Token غير صالح أو تم إلغاؤه" };

            if (storedRefreshToken.ExpiresOn < DateTime.UtcNow)
                return new AuthResultDto { Succeeded = false, Message = "Refresh Token منتهي الصلاحية" };

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null || user.isDeleted)
                return new AuthResultDto { Succeeded = false, Message = "المستخدم غير موجود أو محذوف" };

            await RevokeRefreshTokenAsync(request.RefreshToken, cancellationToken);
            var newToken = await CreateTokenAsync(user, cancellationToken);

            return new AuthResultDto
            {
                Succeeded = true,
                Message = "تم تجديد الرمز بنجاح",
                Token = newToken
            };
        }

        private async Task<UserProfileDto> GetUserProfileAsync(AppUser user, CancellationToken cancellationToken = default)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            var allPermissions = new List<string>();
            foreach (var role in roles)
            {
                var appRole = await _roleManager.FindByNameAsync(role);
                if (appRole != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(appRole);
                    allPermissions.AddRange(roleClaims.Select(c => c.Type));
                }
            }
            allPermissions.AddRange(claims.Select(c => c.Type));
            allPermissions = allPermissions.Distinct().ToList();

            string? authorityName = null;
            if (user.AuthorityId.HasValue)
            {
                authorityName = await _context.Authorities
                    .Where(a => a.Id == user.AuthorityId.Value)
                    .Select(a => a.Name)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            string? userTypeName = null;
            if (user.UserTypeId.HasValue)
            {
                userTypeName = await _context.UserTypes
                    .Where(ut => ut.Id == user.UserTypeId.Value)
                    .Select(ut => ut.TypeName)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                ProfilePictureUrl = user.ProfilePictureUrl,
                UserTypeId = user.UserTypeId,
                UserType = userTypeName,
                AuthorityId = user.AuthorityId,
                AuthorityName = authorityName,
                RoleName = roles.FirstOrDefault(),
                Roles = roles,
                Permissions = allPermissions
            };
        }
    }
}
