namespace testAPI.api.domain.DTOs.OTP
{
    public class OtpResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
        public string? OtpCode { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
    }
}
