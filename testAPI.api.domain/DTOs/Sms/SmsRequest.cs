namespace testAPI.api.domain.DTOs.Sms
{
    public class SmsRequest
    {
        public string To { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Priority { get; set; } = "High";
        public string? CountryCode { get; set; } = "ALL";
    }
}
