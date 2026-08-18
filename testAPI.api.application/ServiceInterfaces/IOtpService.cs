using testAPI.api.domain.DTOs.OTP;

namespace testAPI.api.application.ServiceInterfaces
{
    public interface IOtpService
    {
        Task<OtpResponse> GenerateAndSendOtpAsync(int userId, string phoneNumber, string purpose);
        Task<OtpResponse> VerifyOtpAsync(int userId, string phoneNumber, string otpCode);
        Task<OtpResponse> ValidateOtpForResetAsync(int userId, string phoneNumber, string otpCode);
        Task<bool> IsOtpValidAsync(int userId, string phoneNumber, string otpCode);
    }
}
