using System.ComponentModel.DataAnnotations.Schema;

namespace testAPI.api.infrastructure.Identity
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresOn { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedOn { get; set; }
        public bool IsActive => RevokedOn == null && DateTime.UtcNow <= ExpiresOn;

        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public AppUser User { get; set; } = null!;
    }
}
