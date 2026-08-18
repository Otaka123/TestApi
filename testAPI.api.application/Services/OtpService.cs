using testAPI.api.application.ServiceInterfaces;
using testAPI.api.domain.DTOs.OTP;
using testAPI.api.domain.DTOs.Sms;
using testAPI.api.domain.Entities;
using testAPI.api.infrastructure.Data;
using Application.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace testAPI.api.application.Services
{
    public class OtpService : IOtpService
    {
        private readonly ISmsService _smsService;
        private readonly AppDbContext _context;
        private readonly ILogger<OtpService> _logger;
        private readonly Random _random;

        public OtpService(ISmsService smsService, AppDbContext context, ILogger<OtpService> logger)
        {
            _smsService = smsService;
            _context = context;
            _logger = logger;
            _random = new Random();
        }

        public async Task<OtpResponse> GenerateAndSendOtpAsync(int userId, string phoneNumber, string purpose)
        {
            try
            {
                await CleanOldOtpsAsync(userId);

                string otpCode = "111111"; // للتطوير

                var otpEntity = new OTP
                {
                    Code = otpCode,
                    PhoneNumber = phoneNumber,
                    UserId = userId,
                    CreatedAt = AppDubaiTime.Now,
                    ExpiresAt = AppDubaiTime.Now.AddMinutes(10),
                    Purpose = purpose,
                    IsVerified = false
                };

                _context.OTPs.Add(otpEntity);
                await _context.SaveChangesAsync();

                var message = purpose == "إعادة تعيين كلمة المرور"
                    ? $"كود التحقق لإعادة تعيين كلمة المرور: {otpCode}"
                    : $"كود التحقق الخاص بك: {otpCode}";

                var smsResult = await _smsService.SendSmsAsync(new SmsRequest { To = phoneNumber, Message = message });

                return new OtpResponse
                {
                    Success = smsResult.Success,
                    Message = smsResult.Success ? "تم إرسال كود التحقق بنجاح" : $"فشل إرسال الرسالة: {smsResult.Error}",
                    OtpCode = otpCode,
                    ExpiresAt = otpEntity.ExpiresAt,
                    Error = smsResult.Success ? null : smsResult.Error
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating OTP for user {UserId}", userId);
                return new OtpResponse { Success = false, Error = ex.Message };
            }
        }

        public async Task<OtpResponse> VerifyOtpAsync(int userId, string phoneNumber, string otpCode)
        {
            try
            {
                var otpEntity = await _context.OTPs
                    .Where(o => o.UserId == userId && o.PhoneNumber == phoneNumber && !o.IsVerified)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otpEntity == null)
                    return new OtpResponse { Success = false, Error = "كود التحقق منتهي أو غير موجود" };

                if (otpEntity.ExpiresAt < AppDubaiTime.Now)
                {
                    _context.OTPs.Remove(otpEntity);
                    await _context.SaveChangesAsync();
                    return new OtpResponse { Success = false, Error = "كود التحقق منتهي الصلاحية" };
                }

                if (otpEntity.Code != otpCode)
                    return new OtpResponse { Success = false, Error = "كود التحقق غير صحيح" };

                otpEntity.IsVerified = true;
                otpEntity.VerifiedAt = AppDubaiTime.Now;
                await _context.SaveChangesAsync();

                return new OtpResponse { Success = true, Message = "تم التحقق بنجاح", VerifiedAt = otpEntity.VerifiedAt };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying OTP for user {UserId}", userId);
                return new OtpResponse { Success = false, Error = "حدث خطأ أثناء التحقق" };
            }
        }

        public async Task<OtpResponse> ValidateOtpForResetAsync(int userId, string phoneNumber, string otpCode)
        {
            var otpEntity = await _context.OTPs
                .Where(o => o.UserId == userId && o.PhoneNumber == phoneNumber && o.Code == otpCode)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            if (otpEntity == null || otpEntity.ExpiresAt < AppDubaiTime.Now || !otpEntity.IsVerified)
                return new OtpResponse { Success = false, Error = "كود التحقق غير صالح" };

            return new OtpResponse { Success = true, Message = "التحقق صالح" };
        }

        public async Task<bool> IsOtpValidAsync(int userId, string phoneNumber, string otpCode)
        {
            var result = await VerifyOtpAsync(userId, phoneNumber, otpCode);
            return result.Success;
        }

        private async Task CleanOldOtpsAsync(int userId)
        {
            var expired = await _context.OTPs
                .Where(o => o.UserId == userId && (o.ExpiresAt < AppDubaiTime.Now || o.IsVerified))
                .ToListAsync();

            if (expired.Any())
            {
                _context.OTPs.RemoveRange(expired);
                await _context.SaveChangesAsync();
            }
        }
    }
}
