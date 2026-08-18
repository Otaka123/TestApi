using System.Security.Claims;

namespace testAPI.api.infrastructure.Identity
{
    public static class ClaimStore
    {
        public static List<Claim> VisitClaimsList = new List<Claim>
        {
            new Claim("ViewVisit", "عرض الزيارات"),
            new Claim("EditVisit", "تعديل الزيارات"),
            new Claim("DeleteVisit", "حذف تقييم"),
            new Claim("AddVisit", "اضافه تقييم زياره"),
            new Claim("CreateVisitEvaluation", "انشاء تقييم زياره"),
            new Claim("EditVisitEvaluation", "تعديل تقييم زياره"),
            new Claim("ViewVisitEvaluation", "عرض تقييم زياره"),
            new Claim("signVisit", "توقيع تقرير زيارات"),
            new Claim("UploadVisitAlbum", "رفع البوم الصور"),
            new Claim("UploadMediaItems", "رفع الملفات المتعدده"),
            new Claim("MarkAsFinishedVisit", "تحديد الانتهاء"),
            new Claim("ResetVisit", "اعاده التعيين"),
            new Claim("ViewVisitAverage", "حساب متوسط الزيارات"),
            new Claim("SignatureVisit", "توقيع تقييم زيارات"),
            new Claim("ViewVisitNote", "عرض ملاحظات الزيارة"),
            new Claim("EditVisitNote", "تعديل ملاحظات الزيارة")
        };

        public static List<Claim> HistoryClaimsList = new List<Claim>
        {
            new Claim("ViewHistory", "عرض سجل العمليات"),
            new Claim("ViewHome", "عرض الصفحة الرئيسية"),
            new Claim("ViewCharts", "عرض الرسوم البيانية")
        };

        public static List<Claim> CycleResultsClaimsList = new List<Claim>
        {
            new Claim("ViewAuthorityGeneralResults", "عرض النتائج العامة للجهات"),
            new Claim("ViewEvaluationCharts", "عرض نتائج المحاور الفرعية للجهات")
        };

        public static List<Claim> RolesClaimsList = new List<Claim>
        {
            new Claim("ViewRoles", "عرض الأدوار"),
            new Claim("CreateRole", "إنشاء دور"),
            new Claim("EditRole", "تعديل دور"),
            new Claim("DeleteRole", "حذف دور"),
            new Claim("ManageRoleClaims", "إدارة صلاحيات الدور")
        };

        public static List<Claim> UsersClaimsList = new List<Claim>
        {
            new Claim("ViewUsers", "عرض المستخدمين"),
            new Claim("CreateUser", "إنشاء مستخدم"),
            new Claim("EditUser", "تعديل مستخدم"),
            new Claim("DeleteUser", "حذف مستخدم"),
            new Claim("ResetPassword", "إعادة تعيين كلمة المرور")
        };

        public static List<Claim> MessagesClaimsList = new List<Claim>
        {
            new Claim("ViewMessages", "عرض الرسائل"),
            new Claim("SendMessages", "إرسال الرسائل")
        };

        public static List<Claim> CallClaimsList = new List<Claim>
        {
            new Claim("ViewCall", "عرض مكالمات"),
            new Claim("EditCall", "تعديل مكالمات"),
            new Claim("DeleteCall", "حذف مكالمات"),
            new Claim("AddCall", "اضافه مكالمات"),
            new Claim("CreateCallEvaluation", "انشاء تقييم مكالمه"),
            new Claim("EditCallEvaluation", "تعديل تقييم مكالمه"),
            new Claim("ViewCallEvaluation", "عرض تقييم مكالمه"),
            new Claim("SignCall", "توقيع تقرير مكالمات")
        };

        public static List<Claim> CycleClaimsList = new List<Claim>
        {
            new Claim("ViewCycle", "عرض الدورات"),
            new Claim("CreateCycle", "إنشاء دورة"),
            new Claim("EditCycle", "تعديل دورة"),
            new Claim("DeleteCycle", "حذف دورة")
        };

        public static List<Claim> SettingsClaimsList = new List<Claim>
        {
            new Claim("ViewGeneralSettings", "عرض الإعدادات العامة"),
            new Claim("EditGeneralSettings", "تعديل الإعدادات العامة")
        };

        public static List<Claim> WebsiteClaimsList = new List<Claim>
        {
            new Claim("ViewWebsite", "عرض الموقع"),
            new Claim("EditWebsite", "تعديل الموقع"),
            new Claim("DeleteWebsite", "حذف الموقع"),
            new Claim("AddWebsite", "اضافه الموقع"),
            new Claim("CreateWebSiteEvaluation", "انشاء تقييم الموقع")
        };

        public static List<Claim> ImprovementChance = new List<Claim>
        {
            new Claim("ViewChance", "عرض قائمة فرص التحسين"),
            new Claim("EditChance", "تعديل فرصة تحسين"),
            new Claim("DeleteChance", "حذف فرصة تحسين"),
            new Claim("AddChance", "اضافه فرصة تحسين")
        };

        public static List<Claim> AuthorityReplyClaimsList = new List<Claim>
        {
            new Claim("ViewAuthorityReply", "عرض متابعة رد الجهات"),
            new Claim("AddAuthorityReply", "إضافة رد جهة"),
            new Claim("EditAuthorityReply", "تعديل رد جهة")
        };

        public static List<Claim> AiTrainingClaimsList = new List<Claim>
        {
            new Claim("ViewAiTraining", "عرض تدريب النموذج اللغوي")
        };
    }
}
