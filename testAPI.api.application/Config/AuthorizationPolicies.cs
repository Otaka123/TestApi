namespace testAPI.api.application.Config
{
    public static class AuthorizationPolicies
    {
        public static readonly string[] AllClaimTypes = new[]
        {
            "ViewVisit", "EditVisit", "DeleteVisit", "AddVisit",
            "CreateVisitEvaluation", "EditVisitEvaluation", "ViewVisitEvaluation",
            "signVisit", "UploadVisitAlbum", "UploadMediaItems", "MarkAsFinishedVisit",
            "ResetVisit", "ViewVisitAverage", "SignatureVisit", "ViewVisitNote", "EditVisitNote",
            "ViewHistory", "ViewHome", "ViewCharts",
            "ViewAuthorityGeneralResults", "ViewEvaluationCharts",
            "ViewRoles", "CreateRole", "EditRole", "DeleteRole", "ManageRoleClaims",
            "ViewUsers", "CreateUser", "EditUser", "DeleteUser", "ResetPassword",
            "ViewMessages", "SendMessages",
            "ViewCall", "EditCall", "DeleteCall", "AddCall",
            "CreateCallEvaluation", "EditCallEvaluation", "ViewCallEvaluation",
            "SignCall", "UploadSoundFiles", "MarkAsFinishedCall", "ResetCall",
            "ViewCallAverage", "SignatureCall", "ViewCallNote", "EditCallNote",
            "ViewCycle", "CreateCycle", "EditCycle", "DeleteCycle",
            "ViewGeneralSettings", "EditGeneralSettings",
            "ViewWebsite", "EditWebsite", "DeleteWebsite", "AddWebsite", "CreateWebSiteEvaluation",
            "ViewChance", "EditChance", "DeleteChance", "AddChance",
            "ViewAuthorityReply", "AddAuthorityReply", "EditAuthorityReply",
            "ViewAiTraining"
        };

        public const string SuperAdminPolicy = "SuperAdminPolicy";
        public const string AdminPolicy = "AdminPolicy";
        public const string ViewUsersPolicy = "ViewUsersPolicy";
        public const string CreateUserPolicy = "CreateUserPolicy";
        public const string EditUserPolicy = "EditUserPolicy";
        public const string DeleteUserPolicy = "DeleteUserPolicy";
        public const string ResetPasswordPolicy = "ResetPasswordPolicy";
        public const string ViewRolesPolicy = "ViewRolesPolicy";
        public const string CreateRolePolicy = "CreateRolePolicy";
        public const string EditRolePolicy = "EditRolePolicy";
        public const string DeleteRolePolicy = "DeleteRolePolicy";
        public const string ManageRoleClaimsPolicy = "ManageRoleClaimsPolicy";
        public const string ViewVisitPolicy = "ViewVisitPolicy";
        public const string AddVisitPolicy = "AddVisitPolicy";
        public const string EditVisitPolicy = "EditVisitPolicy";
        public const string DeleteVisitPolicy = "DeleteVisitPolicy";
        public const string ViewVisitEvaluationPolicy = "ViewVisitEvaluationPolicy";
        public const string CreateVisitEvaluationPolicy = "CreateVisitEvaluationPolicy";
        public const string EditVisitEvaluationPolicy = "EditVisitEvaluationPolicy";
        public const string SignatureVisitPolicy = "SignatureVisitPolicy";
        public const string ViewCallPolicy = "ViewCallPolicy";
        public const string AddCallPolicy = "AddCallPolicy";
        public const string EditCallPolicy = "EditCallPolicy";
        public const string DeleteCallPolicy = "DeleteCallPolicy";
        public const string ViewCallEvaluationPolicy = "ViewCallEvaluationPolicy";
        public const string CreateCallEvaluationPolicy = "CreateCallEvaluationPolicy";
        public const string EditCallEvaluationPolicy = "EditCallEvaluationPolicy";
        public const string SignatureCallPolicy = "SignatureCallPolicy";
        public const string ViewWebsitePolicy = "ViewWebsitePolicy";
        public const string AddWebsitePolicy = "AddWebsitePolicy";
        public const string EditWebsitePolicy = "EditWebsitePolicy";
        public const string DeleteWebsitePolicy = "DeleteWebsitePolicy";
        public const string CreateWebSiteEvaluationPolicy = "CreateWebSiteEvaluationPolicy";
        public const string ViewCyclePolicy = "ViewCyclePolicy";
        public const string CreateCyclePolicy = "CreateCyclePolicy";
        public const string EditCyclePolicy = "EditCyclePolicy";
        public const string DeleteCyclePolicy = "DeleteCyclePolicy";
        public const string ViewGeneralSettingsPolicy = "ViewGeneralSettingsPolicy";
        public const string EditGeneralSettingsPolicy = "EditGeneralSettingsPolicy";
        public const string ViewMessagesPolicy = "ViewMessagesPolicy";
        public const string SendMessagesPolicy = "SendMessagesPolicy";
        public const string ViewHistoryPolicy = "ViewHistoryPolicy";
        public const string ViewHomePolicy = "ViewHomePolicy";
        public const string ViewChartsPolicy = "ViewChartsPolicy";
    }
}
