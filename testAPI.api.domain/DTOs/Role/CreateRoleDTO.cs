using System.ComponentModel.DataAnnotations;

namespace testAPI.api.domain.DTOs.Role
{
    public class CreateRoleDTO
    {
        [Required(ErrorMessage = "اسم الدور مطلوب")]
        public string Name { get; set; } = string.Empty;
    }
}
