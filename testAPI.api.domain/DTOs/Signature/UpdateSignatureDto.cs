using Microsoft.AspNetCore.Http;

namespace testAPI.api.domain.DTOs.Signature
{
    public class UpdateSignatureDto
    {
        public int Id { get; set; }
        public IFormFile? SignatureImage { get; set; }
    }
}
