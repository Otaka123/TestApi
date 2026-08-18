using Application.Helpers;

namespace testAPI.api.domain.Entities
{
    public class Signature
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = AppDubaiTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
