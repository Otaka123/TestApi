namespace testAPI.api.domain.DTOs.Role
{
    public class RoleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
    }
}
