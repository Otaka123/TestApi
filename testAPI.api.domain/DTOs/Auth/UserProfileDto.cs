namespace testAPI.api.domain.DTOs.Auth
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public int? UserTypeId { get; set; }
        public string? UserType { get; set; }
        public int? AuthorityId { get; set; }
        public string? AuthorityName { get; set; }
        public string? RoleName { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public IList<string> Permissions { get; set; } = new List<string>();
    }
}
