using System.Collections.Immutable;
using System.Security.Claims;
using AuthServer.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static AuthServer.Services.AuthorizationService;

namespace AuthServer.Endpoints;

public static class AuthorizationEndpoint
{
    public static void MapAuthorizationEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapMethods("~/connect/authorize", [HttpMethods.Get, HttpMethods.Post], HandleAuthorization);
        app.MapPost("~/connect/token", HandleTokenExchange);
        app.MapMethods("~/connect/logout", [HttpMethods.Get, HttpMethods.Post], async (HttpContext context) =>
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Results.SignOut(
                new()
                {
                    RedirectUri = "/",
                },
                [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]
            );
        });
    }

    private static async Task<IResult> HandleAuthorization(
        ILogger<Program> logger,
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictScopeManager scopeManager,
        IOpenIddictAuthorizationManager authorizationManager
    )
    {
        var request = context.GetOpenIddictServerRequest();

        if (request is null)
            return Results.BadRequest(new OpenIddictResponse
            {
                Error = Errors.InvalidRequest,
                ErrorDescription = "The OpenID Connect request cannot be retrieved.",
            });

        var parameters = ParseOAuthParameters(context, [Parameters.Prompt]);

        var result = await context.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        if (!IsAuthenticated(result, request))
            return Results.Challenge(
                new()
                {
                    RedirectUri = BuildRedirectUrl(context.Request, parameters),
                },
                [CookieAuthenticationDefaults.AuthenticationScheme]
            );

        var app = await applicationManager.FindByClientIdAsync(request.ClientId ?? "");

        if (app is null)
            return Results.NotFound(new OpenIddictResponse
            {
                Error = Errors.InvalidClient,
                ErrorDescription = "The specified client was not found.",
            });

        var permissions = await applicationManager.GetPermissionsAsync(app);

        var audiences = permissions.Where(x =>
                x.StartsWith("scp") && !x.EndsWith("email") && !x.EndsWith("profile") && !x.EndsWith("roles"))
            .Select(x => x["scp:".Length..])
            .ToImmutableArray();

        var userEmail = result.Principal?.FindFirst(ClaimTypes.Email)!.Value ?? string.Empty;

        var user = await userManager.FindByEmailAsync(userEmail);

        if (user is null)
            return Results.NotFound(new OpenIddictResponse
            {
                Error = Errors.LoginRequired,
                ErrorDescription = "We couldn't find the requested user.",
            });

        //  Retrieve the permanent authorizations associated with the user and the calling client application.
        var authorizations = await authorizationManager.FindAsync(
            await userManager.GetUserIdAsync(user),
            await applicationManager.GetIdAsync(app),
            Statuses.Valid,
            AuthorizationTypes.Permanent,
            request.GetScopes()).ToListAsync();

        // Note: the same check is already made in the other action but is repeated
        // here to ensure a malicious user can't abuse this POST-only endpoint and
        // force it to return a valid response without the external authorization.
        if (authorizations.Count is 0 && await applicationManager.HasConsentTypeAsync(app, ConsentTypes.External))
            return Results.Forbid(
                authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                properties: new(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The logged in user is not allowed to access this client application.",
                }));

        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            ClaimTypes.Name,
            ClaimTypes.Role
        );

        identity.SetClaim(Claims.Subject, user.Id.ToString())
            .SetClaim(Claims.Email, user.Email)
            .SetClaim(Claims.Name, user.UserName)
            .SetClaims(Claims.Audience, audiences)
            .SetClaims(Claims.Role, [..await userManager.GetRolesAsync(user)]);

        identity.SetScopes(request.GetScopes());

        identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

        // Automatically create a permanent authorization to avoid requiring explicit consent
        // for future authorization or token requests containing the same scopes.
        var authorization = authorizations.LastOrDefault();
        authorization ??= await authorizationManager.CreateAsync(
            identity,
            user.Id.ToString(),
            (await applicationManager.GetIdAsync(app))!,
            AuthorizationTypes.Permanent,
            identity.GetScopes());

        identity.SetAuthorizationId(await authorizationManager.GetIdAsync(authorization));
        identity.SetDestinations(c => GetDestinations(identity, c));

        return Results.SignIn(
            new(identity),
            null,
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme
        );
    }

    private static async Task<IResult> HandleTokenExchange(
        HttpContext context,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOpenIddictScopeManager scopeManager,
        ILogger<Program> logger
    )
    {
        var request = context.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (!request.IsAuthorizationCodeGrantType() && !request.IsRefreshTokenGrantType())
            return Results.BadRequest(
                new OpenIddictResponse
                {
                    Error = Errors.UnsupportedGrantType,
                    ErrorDescription = "The specified grant type is not supported.",
                }
            );

        var result = await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        var userId = result.Principal?.GetClaim(Claims.Subject);

        if (userId is null)
            return Results.UnprocessableEntity(new OpenIddictResponse
            {
                Error = Errors.InvalidRequest,
                ErrorDescription = "The specified user id was not found.",
            });

        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Results.NotFound(new OpenIddictResponse
            {
                Error = Errors.LoginRequired,
                ErrorDescription = "We couldn't find the requested user.",
            });

        // Ensure the user is still allowed to sign in.
        if (!await signInManager.CanSignInAsync(user))
            return Results.Forbid(
                authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                properties: new(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The user is no longer allowed to sign in.",
                }));

        if (string.IsNullOrEmpty(userId))
            return Results.Forbid(
                authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                properties: new(new Dictionary<string, string?>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "Cannot find user from token",
                })
            );

        var identity = new ClaimsIdentity(
            TokenValidationParameters.DefaultAuthenticationType,
            ClaimTypes.Name,
            ClaimTypes.Role
        );

        // Override the user claims present in the principal in case they
        // changed since the authorization code/refresh token was issued.
        identity.SetClaim(Claims.Subject, userId)
            .SetClaim(Claims.Email, user.Email)
            .SetClaim(Claims.Name, user.UserName)
            .SetClaim(Claims.Name, user.UserName)
            .SetClaims(Claims.Role, [..await userManager.GetRolesAsync(user)]);

        identity.SetDestinations(c => GetDestinations(identity, c));

        identity.SetScopes(request.GetScopes());

        identity.SetResources(await scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens
        return Results.SignIn(new(identity), null, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}