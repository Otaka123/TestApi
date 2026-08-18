namespace testAPI.api.domain.DTOs.Auth
{
    public class TokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpireAt { get; set; }
        public DateTime RefreshTokenExpireAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public UserProfileDto? User { get; set; }
    }
}
