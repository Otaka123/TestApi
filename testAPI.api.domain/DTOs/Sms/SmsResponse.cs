namespace testAPI.api.domain.DTOs.Sms
{
    public class SmsResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Error { get; set; }
        public string? Response { get; set; }
    }
}
