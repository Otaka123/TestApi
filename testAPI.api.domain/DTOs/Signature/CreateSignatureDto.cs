using Microsoft.AspNetCore.Http;

namespace testAPI.api.domain.DTOs.Signature
{
    public class CreateSignatureDto
    {
        public int UserId { get; set; }
        public IFormFile SignatureImage { get; set; } = null!;
    }
}
