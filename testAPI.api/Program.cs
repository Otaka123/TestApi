using testAPI.api.application.Config;
using testAPI.api.domain.DTOs.Sms;
using testAPI.api.infrastructure.Data;
using testAPI.api.infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Security.Claims;
using System.Text;

namespace testAPI.api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT:Key is not configured in appsettings.json"));

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null
                )));

        builder.Services.AddIdentity<AppUser, AppRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 1;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = bool.TryParse(jwtSettings["ValidateIssuer"], out var vi) && vi,
                ValidateAudience = bool.TryParse(jwtSettings["ValidateAudience"], out var va) && va,
                ValidateLifetime = bool.TryParse(jwtSettings["ValidateLifetime"], out var vl) && vl,
                ValidateIssuerSigningKey = bool.TryParse(jwtSettings["ValidateIssuerSigningKey"], out var vis) && vis,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name
            };
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        context.Response.Headers["Token-Expired"] = "true";
                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddAuthorizationBuilder()
            .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .Build())
            .AddPolicy(AuthorizationPolicies.SuperAdminPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireRole("superadmin"))
            .AddPolicy(AuthorizationPolicies.AdminPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireRole("superadmin", "admin"))
            .AddPolicy(AuthorizationPolicies.ViewUsersPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("ViewUsers"))
            .AddPolicy(AuthorizationPolicies.CreateUserPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("CreateUser"))
            .AddPolicy(AuthorizationPolicies.EditUserPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("EditUser"))
            .AddPolicy(AuthorizationPolicies.DeleteUserPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("DeleteUser"))
            .AddPolicy(AuthorizationPolicies.ResetPasswordPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("ResetPassword"))
            .AddPolicy(AuthorizationPolicies.ViewRolesPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("ViewRoles"))
            .AddPolicy(AuthorizationPolicies.CreateRolePolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("CreateRole"))
            .AddPolicy(AuthorizationPolicies.EditRolePolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("EditRole"))
            .AddPolicy(AuthorizationPolicies.DeleteRolePolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("DeleteRole"))
            .AddPolicy(AuthorizationPolicies.ManageRoleClaimsPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("ManageRoleClaims"))
            .AddPolicy(AuthorizationPolicies.ViewVisitPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("ViewVisit"))
            .AddPolicy(AuthorizationPolicies.AddVisitPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("AddVisit"))
            .AddPolicy(AuthorizationPolicies.EditVisitPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("EditVisit"))
            .AddPolicy(AuthorizationPolicies.DeleteVisitPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("DeleteVisit"))
            .AddPolicy(AuthorizationPolicies.ViewVisitEvaluationPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("ViewVisitEvaluation"))
            .AddPolicy(AuthorizationPolicies.CreateVisitEvaluationPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("CreateVisitEvaluation"))
            .AddPolicy(AuthorizationPolicies.EditVisitEvaluationPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("EditVisitEvaluation"))
            .AddPolicy(AuthorizationPolicies.SignatureVisitPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("SignatureVisit", "signVisit"))
            .AddPolicy(AuthorizationPolicies.ViewCallPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("ViewCall"))
            .AddPolicy(AuthorizationPolicies.AddCallPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("AddCall"))
            .AddPolicy(AuthorizationPolicies.EditCallPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("EditCall"))
            .AddPolicy(AuthorizationPolicies.DeleteCallPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("DeleteCall"))
            .AddPolicy(AuthorizationPolicies.ViewCallEvaluationPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("ViewCallEvaluation"))
            .AddPolicy(AuthorizationPolicies.CreateCallEvaluationPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("CreateCallEvaluation"))
            .AddPolicy(AuthorizationPolicies.EditCallEvaluationPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("EditCallEvaluation"))
            .AddPolicy(AuthorizationPolicies.SignatureCallPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("SignatureCall", "SignCall"))
            .AddPolicy(AuthorizationPolicies.ViewWebsitePolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("ViewWebsite"))
            .AddPolicy(AuthorizationPolicies.AddWebsitePolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("AddWebsite"))
            .AddPolicy(AuthorizationPolicies.EditWebsitePolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("EditWebsite"))
            .AddPolicy(AuthorizationPolicies.DeleteWebsitePolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("DeleteWebsite"))
            .AddPolicy(AuthorizationPolicies.CreateWebSiteEvaluationPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("CreateWebSiteEvaluation"))
            .AddPolicy(AuthorizationPolicies.ViewCyclePolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("ViewCycle"))
            .AddPolicy(AuthorizationPolicies.CreateCyclePolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("CreateCycle"))
            .AddPolicy(AuthorizationPolicies.EditCyclePolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("EditCycle"))
            .AddPolicy(AuthorizationPolicies.DeleteCyclePolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("DeleteCycle"))
            .AddPolicy(AuthorizationPolicies.ViewGeneralSettingsPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("ViewGeneralSettings"))
            .AddPolicy(AuthorizationPolicies.EditGeneralSettingsPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("EditGeneralSettings"))
            .AddPolicy(AuthorizationPolicies.ViewMessagesPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("ViewMessages"))
            .AddPolicy(AuthorizationPolicies.SendMessagesPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("SendMessages"))
            .AddPolicy(AuthorizationPolicies.ViewHistoryPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("ViewHistory"))
            .AddPolicy(AuthorizationPolicies.ViewHomePolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("ViewHome"))
            .AddPolicy(AuthorizationPolicies.ViewChartsPolicy, p => p
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireClaim("ViewCharts"));

        builder.Services.Configure<SmsSettings>(builder.Configuration.GetSection("SmsSettings"));
        builder.Services.Configure<SecurityStampValidatorOptions>(options =>
        {
            options.ValidationInterval = TimeSpan.FromMinutes(10);
        });

        // ✅ استخدام المشاريع المستقلة الجديدة فقط
        builder.Services.AddApiApplicationServices();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });
        });

        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("GlobalSliding", httpContext =>
                System.Threading.RateLimiting.RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "global",
                    factory: _ => new System.Threading.RateLimiting.SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        SegmentsPerWindow = 4,
                        QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                        QueueLimit = 10
                    }));
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    StatusCode = StatusCodes.Status429TooManyRequests,
                    Message = "Too many requests. Please try again later."
                }, cancellationToken: token);
            };
        });

        builder.Services.AddOpenApi();

        builder.WebHost.ConfigureKestrel(options =>
        {
            options.AddServerHeader = false;
        });

        var app = builder.Build();

        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Remove("Server");
                context.Response.Headers.Remove("X-Powered-By");
                context.Response.Headers.Remove("X-AspNet-Version");
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                return Task.CompletedTask;
            });
            await next();
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseCors("AllowAll");
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapMethods("/health", new[] { "GET", "HEAD" }, (HttpContext context) =>
        {
            if (context.Request.Method == HttpMethods.Head)
                return Results.StatusCode(StatusCodes.Status200OK);
            return Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
        }).AllowAnonymous();

        await testAPI.api.infrastructure.Data.RoleSeeder.SeedRolesAndUsersAsync(app.Services);

        app.Run();
    }
}
