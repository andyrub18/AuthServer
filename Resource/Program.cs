using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;
using Resource;
using Resource.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Get the security configuration constants
var securityConfig = builder.Configuration.GetSection("SecurityConfig").Get<SecurityConfig>() ??
                     throw new NullReferenceException("SecurityConfig is null");

// Add OpenIddict validation
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer(securityConfig.Issuer);
        options.AddAudiences(securityConfig.Audience);

        options.AddEncryptionKey(new SymmetricSecurityKey(
            Convert.FromBase64String(securityConfig.Key)));

        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });

// Add authentication and authorization
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Constants.AuthPolicy,
        policy => policy.RequireRole("Editor"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapResourceEndpoints();
app.UseHttpsRedirection();

app.Run();