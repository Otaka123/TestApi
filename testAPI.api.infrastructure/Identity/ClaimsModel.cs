using testAPI.api.domain.Entities;

namespace testAPI.api.infrastructure.Identity
{
    public class ClaimsModel
    {
        public int RoleId { get; set; }
        public List<ClaimSelection> HomeClaimsList { get; set; } = new();
        public List<ClaimSelection> RolesClaimsList { get; set; } = new();
        public List<ClaimSelection> UsersClaimsList { get; set; } = new();
    }
}
