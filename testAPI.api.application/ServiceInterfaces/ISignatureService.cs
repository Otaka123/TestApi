using testAPI.api.domain.DTOs.Signature;

namespace testAPI.api.application.ServiceInterfaces
{
    public interface ISignatureService
    {
        Task<SignatureDto> AddSignatureAsync(CreateSignatureDto createDto, CancellationToken cancellationToken = default);
        Task<SignatureDto> UpdateSignatureAsync(UpdateSignatureDto updateDto, CancellationToken cancellationToken = default);
        Task<SignatureDto?> GetSignatureByUserIdAsync(int userId, CancellationToken cancellationToken = default);
        Task<SignatureDto?> GetSignatureByIdAsync(int id, CancellationToken cancellationToken = default);
        Task SendUpdateOtpAsync(int userId);
        Task<SignatureDto> VerifyOtpAndUpdateSignatureAsync(UpdateSignatureDto updateDto, string otpCode, CancellationToken cancellationToken = default);
    }
}
