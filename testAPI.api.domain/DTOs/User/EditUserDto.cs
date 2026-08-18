namespace testAPI.api.domain.DTOs.User
{
    public class EditUserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
        public int UserTypeId { get; set; }
        public int RoleId { get; set; }
        public int? AuthorityId { get; set; }
        public int? DeveloperCode { get; set; }
    }
}
