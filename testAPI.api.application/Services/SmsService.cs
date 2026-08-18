using testAPI.api.application.ServiceInterfaces;
using testAPI.api.domain.DTOs.Sms;
using Microsoft.Extensions.Logging;

namespace testAPI.api.application.Services
{
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;
        private readonly HttpClient _httpClient;

        public SmsService(ILogger<SmsService> logger)
        {
            _logger = logger;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(100) };
        }

        public async Task<SmsResponse> SendSmsAsync(SmsRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.To))
                    return new SmsResponse { Success = false, Error = "رقم الهاتف مطلوب" };

                if (string.IsNullOrWhiteSpace(request.Message))
                    return new SmsResponse { Success = false, Error = "نص الرسالة مطلوب" };

                var cleanPhoneNumber = new string(request.To.Where(char.IsDigit).ToArray());

                if (cleanPhoneNumber.Length < 9)
                    return new SmsResponse { Success = false, Error = "رقم الهاتف غير صالح" };

                if (cleanPhoneNumber.StartsWith("0"))
                    cleanPhoneNumber = cleanPhoneNumber.Substring(1);

                if (!cleanPhoneNumber.StartsWith("971"))
                    cleanPhoneNumber = "971" + cleanPhoneNumber;

                string url = $"http://smsapi.esmart-vision.com/api/mim/SendSMS" +
                             $"?userid=SMV2300001&pwd=DD8D$803_C91" +
                             $"&mobile={Uri.EscapeDataString(cleanPhoneNumber)}" +
                             $"&sender=FFRD" +
                             $"&msg={Uri.EscapeDataString(request.Message)}" +
                             $"&msgtype=20";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    return new SmsResponse { Success = true, Response = responseBody, Message = "تم إرسال الرسالة بنجاح" };
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return new SmsResponse { Success = false, Error = $"فشل إرسال الرسالة: {response.StatusCode}", Response = errorContent };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS");
                return new SmsResponse { Success = false, Error = ex.Message };
            }
        }

        public async Task<SmsResponse> SendOtpAsync(string phoneNumber, string otpCode, string purpose = "التحقق")
        {
            var message = purpose switch
            {
                "تسجيل الدخول" => $"كود التحقق لتسجيل الدخول: {otpCode}",
                "إعادة تعيين كلمة المرور" => $"كود التحقق لإعادة تعيين كلمة المرور: {otpCode}",
                _ => $"كود التحقق الخاص بك: {otpCode}"
            };

            return await SendSmsAsync(new SmsRequest { To = phoneNumber, Message = message });
        }
    }
}
