using System.ComponentModel.DataAnnotations;

namespace testAPI.api.domain.DTOs.Auth
{
    public class RefreshTokenRequestDto
    {
        [Required(ErrorMessage = "Access Token مطلوب")]
        public string AccessToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Refresh Token مطلوب")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
