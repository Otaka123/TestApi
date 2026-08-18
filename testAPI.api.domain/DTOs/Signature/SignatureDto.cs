namespace testAPI.api.domain.DTOs.Signature
{
    public class SignatureDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
