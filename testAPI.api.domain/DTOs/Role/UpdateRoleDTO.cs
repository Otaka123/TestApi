using System.ComponentModel.DataAnnotations;

namespace testAPI.api.domain.DTOs.Role
{
    public class UpdateRoleDTO
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "اسم الدور مطلوب")]
        public string Name { get; set; } = string.Empty;
    }
}
