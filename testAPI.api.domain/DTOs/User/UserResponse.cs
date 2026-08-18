namespace testAPI.api.domain.DTOs.User
{
    public class UserResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
    }
}
