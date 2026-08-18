using Application.Helpers;

namespace testAPI.api.domain.Entities
{
    public class OTP
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = AppDubaiTime.Now;
        public DateTime ExpiresAt { get; set; }
        public bool IsVerified { get; set; } = false;
        public DateTime? VerifiedAt { get; set; }
    }
}
