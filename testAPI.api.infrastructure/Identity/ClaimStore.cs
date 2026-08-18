using System.Security.Claims;

namespace testAPI.api.infrastructure.Identity
{
    public static class ClaimStore
    {
        // صلاحيات الصفحة الرئيسية
        public static List<Claim> HomeClaimsList = new List<Claim>
        {
            new Claim("ViewHome", "عرض الصفحة الرئيسية")
        };

        // صلاحيات إدارة الأدوار
        public static List<Claim> RolesClaimsList = new List<Claim>
        {
            new Claim("ViewRoles", "عرض الأدوار"),
            new Claim("CreateRole", "إنشاء دور"),
            new Claim("EditRole", "تعديل دور"),
            new Claim("DeleteRole", "حذف دور"),
            new Claim("ManageRoleClaims", "إدارة صلاحيات الدور")
        };

        // صلاحيات إدارة المستخدمين
        public static List<Claim> UsersClaimsList = new List<Claim>
        {
            new Claim("ViewUsers", "عرض المستخدمين"),
            new Claim("CreateUser", "إنشاء مستخدم"),
            new Claim("EditUser", "تعديل مستخدم"),
            new Claim("DeleteUser", "حذف مستخدم"),
            new Claim("ResetPassword", "إعادة تعيين كلمة المرور")
        };
    }
}
