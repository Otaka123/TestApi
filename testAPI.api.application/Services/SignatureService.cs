using testAPI.api.application.ServiceInterfaces;
using testAPI.api.domain.DTOs.Signature;
using testAPI.api.domain.Entities;
using testAPI.api.infrastructure.Identity;
using testAPI.api.infrastructure.Persistence.Interface;
using Application.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace testAPI.api.application.Services
{
    public class SignatureService : ISignatureService
    {
        private readonly ISignatureRepository _signatureRepository;
        private readonly IFileService _fileService;
        private readonly ILogger<SignatureService> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly IOtpService _otpService;

        public SignatureService(
            ISignatureRepository signatureRepository,
            IFileService fileService,
            ILogger<SignatureService> logger,
            UserManager<AppUser> userManager,
            IOtpService otpService)
        {
            _signatureRepository = signatureRepository;
            _fileService = fileService;
            _logger = logger;
            _userManager = userManager;
            _otpService = otpService;
        }

        public async Task<SignatureDto> AddSignatureAsync(CreateSignatureDto createDto, CancellationToken cancellationToken = default)
        {
            if (!_fileService.IsValidImage(createDto.SignatureImage))
                throw new InvalidOperationException("ملف غير صالح");

            var imagePath = await _fileService.SaveImageAsync(createDto.SignatureImage, "signatures");
            if (string.IsNullOrEmpty(imagePath))
                throw new InvalidOperationException("فشل في حفظ صورة التوقيع");

            var signature = new Signature
            {
                UserId = createDto.UserId,
                ImagePath = imagePath,
                CreatedAt = AppDubaiTime.Now
            };

            await _signatureRepository.CreateAsync(signature, cancellationToken);
            return MapToDto(signature);
        }

        public async Task<SignatureDto> UpdateSignatureAsync(UpdateSignatureDto updateDto, CancellationToken cancellationToken = default)
        {
            var existingSignature = await _signatureRepository.GetAsync(s => s.Id == updateDto.Id, cancellationToken: cancellationToken);
            if (existingSignature == null)
                throw new KeyNotFoundException("التوقيع غير موجود");

            if (updateDto.SignatureImage != null)
            {
                if (!_fileService.IsValidImage(updateDto.SignatureImage))
                    throw new InvalidOperationException("ملف غير صالح");

                var newImagePath = await _fileService.SaveImageAsync(updateDto.SignatureImage, "signatures");
                if (string.IsNullOrEmpty(newImagePath))
                    throw new InvalidOperationException("فشل في حفظ صورة التوقيع الجديدة");

                existingSignature.ImagePath = newImagePath;
            }

            existingSignature.UpdatedAt = AppDubaiTime.Now;
            await _signatureRepository.UpdateSignatureAsync(existingSignature, cancellationToken);
            return MapToDto(existingSignature);
        }

        public async Task<SignatureDto?> GetSignatureByUserIdAsync(int userId, CancellationToken cancellationToken = default)
        {
            var signature = await _signatureRepository.GetByUserIdAsync(userId, cancellationToken);
            return signature == null ? null : MapToDto(signature);
        }

        public async Task<SignatureDto?> GetSignatureByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var signature = await _signatureRepository.GetAsync(s => s.Id == id, cancellationToken: cancellationToken);
            return signature == null ? null : MapToDto(signature);
        }

        public async Task SendUpdateOtpAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) throw new InvalidOperationException("المستخدم غير موجود");
            if (string.IsNullOrEmpty(user.PhoneNumber)) throw new InvalidOperationException("لا يوجد رقم هاتف");

            var otpResult = await _otpService.GenerateAndSendOtpAsync(userId, user.PhoneNumber, "تحديث التوقيع");
            if (!otpResult.Success)
                throw new InvalidOperationException(otpResult.Error ?? "فشل في إرسال كود التحقق");
        }

        public async Task<SignatureDto> VerifyOtpAndUpdateSignatureAsync(UpdateSignatureDto updateDto, string otpCode, CancellationToken cancellationToken = default)
        {
            var existingSignature = await _signatureRepository.GetAsync(s => s.Id == updateDto.Id, cancellationToken: cancellationToken);
            if (existingSignature == null)
                throw new KeyNotFoundException("التوقيع غير موجود");

            var user = await _userManager.FindByIdAsync(existingSignature.UserId.ToString());
            if (user == null || string.IsNullOrEmpty(user.PhoneNumber))
                throw new InvalidOperationException("المستخدم غير موجود أو لا يوجد رقم هاتف");

            var otpVerification = await _otpService.VerifyOtpAsync(existingSignature.UserId, user.PhoneNumber, otpCode);
            if (!otpVerification.Success)
                throw new InvalidOperationException("كود التحقق غير صحيح أو منتهي الصلاحية");

            return await UpdateSignatureAsync(updateDto, cancellationToken);
        }

        private SignatureDto MapToDto(Signature signature) => new SignatureDto
        {
            Id = signature.Id,
            UserId = signature.UserId,
            ImagePath = signature.ImagePath,
            CreatedAt = signature.CreatedAt,
            UpdatedAt = signature.UpdatedAt
        };
    }
}
