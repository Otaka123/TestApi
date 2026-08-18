using testAPI.api.domain.Entities;

namespace testAPI.api.infrastructure.Identity
{
    public class ClaimsModel
    {
        public int RoleId { get; set; }
        public List<ClaimSelection> VisitClaimsList { get; set; } = new();
        public List<ClaimSelection> HistoryClaimsList { get; set; } = new();
        public List<ClaimSelection> CycleResultsClaimsList { get; set; } = new();
        public List<ClaimSelection> RolesClaimsList { get; set; } = new();
        public List<ClaimSelection> UsersClaimsList { get; set; } = new();
        public List<ClaimSelection> MessagesClaimsList { get; set; } = new();
        public List<ClaimSelection> CallClaimsList { get; set; } = new();
        public List<ClaimSelection> WebsiteClaimsList { get; set; } = new();
        public List<ClaimSelection> ImprovementChance { get; set; } = new();
        public List<ClaimSelection> AuthorityReplyClaimsList { get; set; } = new();
        public List<ClaimSelection> CycleClaimsList { get; set; } = new();
        public List<ClaimSelection> SettingsClaimsList { get; set; } = new();
        public List<ClaimSelection> AiTrainingClaimsList { get; set; } = new();
    }
}
