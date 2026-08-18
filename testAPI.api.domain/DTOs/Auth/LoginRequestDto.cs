using System.ComponentModel.DataAnnotations;

namespace testAPI.api.domain.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }
}
