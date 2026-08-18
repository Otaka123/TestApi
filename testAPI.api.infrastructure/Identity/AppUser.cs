using testAPI.api.domain.Entities;
using testAPI.api.domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace testAPI.api.infrastructure.Identity
{
    public class AppUser : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
        public bool isDeleted { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public TwoFactorPolicy TwoFactorPolicy { get; set; } = TwoFactorPolicy.UserChoice;
        public bool HasCompletedEnforcedSetup { get; set; }
        public int? DeveloperCode { get; set; }

        public ICollection<History> Histories { get; set; } = new List<History>();
        public ICollection<Signature> Signatures { get; set; } = new List<Signature>();

        public int? UserTypeId { get; set; }
        [ForeignKey("UserTypeId")]
        public UserType? UserType { get; set; }

        public int? AuthorityId { get; set; }
        [ForeignKey("AuthorityId")]
        public Authority? Authority { get; set; }

        public AppUser()
        {
        }
    }
}
