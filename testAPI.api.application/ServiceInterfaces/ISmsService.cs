using testAPI.api.domain.DTOs.Sms;

namespace testAPI.api.application.ServiceInterfaces
{
    public interface ISmsService
    {
        Task<SmsResponse> SendSmsAsync(SmsRequest request);
        Task<SmsResponse> SendOtpAsync(string phoneNumber, string otpCode, string purpose = "التحقق");
    }
}
