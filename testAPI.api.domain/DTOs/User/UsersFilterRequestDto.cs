namespace testAPI.api.domain.DTOs.User
{
    public class UsersFilterRequestDto
    {
        public int? RoleId { get; set; }
        public int? UserTypeId { get; set; }
        public int? UserId { get; set; }
    }
}
