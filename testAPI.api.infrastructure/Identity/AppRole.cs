using Microsoft.AspNetCore.Identity;

namespace testAPI.api.infrastructure.Identity
{
    public class AppRole : IdentityRole<int>
    {
        public bool isDeleted { get; set; }
    }
}
