namespace testAPI.api.domain.Entities
{
    public class ClaimSelection
    {
        public string ClaimType { get; set; } = string.Empty;
        public string? Label { get; set; }
        public bool IsSelected { get; set; }
    }
}
