namespace testAPI.api.domain.DTOs.Auth
{
    public class AuthResultDto
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public IEnumerable<string>? Errors { get; set; }
        public TokenResponseDto? Token { get; set; }
    }
}
