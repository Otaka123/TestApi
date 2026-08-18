namespace testAPI.api.domain.DTOs.User
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int? DeveloperCode { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public int? UserTypeId { get; set; }
        public string? UserType { get; set; }
        public bool? IsLocked { get; set; }
        public bool? IsTwoFactorEnabled { get; set; }
        public bool IsDeleted { get; set; }
        public int? AuthorityId { get; set; }
    }
}
