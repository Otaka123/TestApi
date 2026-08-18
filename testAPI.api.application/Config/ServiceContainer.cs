using testAPI.api.application.ServiceInterfaces;
using testAPI.api.application.Services;
using testAPI.api.infrastructure.Config;
using Microsoft.Extensions.DependencyInjection;

namespace testAPI.api.application.Config
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddApiApplicationServices(this IServiceCollection services)
        {
            // Infrastructure repositories
            services.AddApiInfrastructureRepositories();

            // Application services
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IRoleClaimsService, RoleClaimsService>();
            services.AddScoped<IExcelExportService, ExcelExportService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<ISignatureService, SignatureService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<ISmsService, SmsService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddHttpContextAccessor();

            return services;
        }
    }
}
