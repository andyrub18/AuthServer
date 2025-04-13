using AuthServer.Data;
using AuthServer.Data.Repository;
using AuthServer.Models;
using AuthServer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace AuthServer.Extensions;

public static class IdentityServiceRegistration
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add services to the container.
        var connectionString = configuration.GetConnectionString("identity") ??
                               throw new InvalidOperationException("Connection string 'identity' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
            options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            options.UseOpenIddict();
        });

        services.AddDatabaseDeveloperPageExceptionFilter();

        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
            })
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<ApplicationDbContext>();
            })
            .AddServer(options =>
            {
                options.AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow()
                    .RequireProofKeyForCodeExchange()
                    .SetAuthorizationEndpointUris("connect/authorize")
                    .SetEndSessionEndpointUris("connect/logout")
                    .SetTokenEndpointUris("connect/token");

                options.IgnoreEndpointPermissions()
                    .IgnoreGrantTypePermissions();

                options.RegisterScopes(
                    Scopes.Email,
                    Scopes.Profile,
                    Scopes.Roles,
                    Scopes.OfflineAccess,
                    Scopes.OpenId
                );

                options.AddEncryptionKey(new SymmetricSecurityKey(
                    Convert.FromBase64String(configuration["securityKey"]!)
                ));

                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough()
                    .EnableTokenEndpointPassthrough();

                options.SetAccessTokenLifetime(TimeSpan.FromHours(1))
                    .SetRefreshTokenLifetime(TimeSpan.FromDays(7));
            }).AddClient(options =>
            {
                options.AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow();

                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                options.UseAspNetCore()
                    .EnableRedirectionEndpointPassthrough();

                options.UseSystemNetHttp();

                // Register the Google integration.
                options.UseWebProviders().AddGoogle(opt =>
                {
                    opt.SetClientId("client_id")
                        .SetClientSecret("client_secrets")
                        .SetRedirectUri("/signin-google")
                        .SetProviderDisplayName("Sign In With Google")
                        .AddScopes("email profile");
                });
            }).AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        services.AddSingleton<IEmailSender, EmailSenderService>();

        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(c => c.LoginPath = "/Account/Login");

        services.AddScoped<ClientAppRepository>();
        services.AddScoped<ScopesRepository>();

        return services;
    }
}