using testAPI.api.infrastructure.Identity;
using testAPI.api.infrastructure.Persistence;
using testAPI.api.infrastructure.Persistence.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using static testAPI.api.infrastructure.Persistence.Interface.IRepo;

namespace testAPI.api.infrastructure.Config
{
    public static class InfrastructureContainer
    {
        public static IServiceCollection AddApiInfrastructureRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepo<>), typeof(Repo.Repository<>));
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ISignatureRepository, SignatureRepository>();
            services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppUserClaimsPrincipalFactory>();

            return services;
        }
    }
}
