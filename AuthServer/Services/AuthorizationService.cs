using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Primitives;
using OpenIddict.Abstractions;

namespace AuthServer.Services;

public static class AuthorizationService
{
    public static IDictionary<string, StringValues> ParseOAuthParameters(HttpContext context,
        List<string>? excluding = null)
    {
        excluding ??= [];

        var parameters = context.Request.HasFormContentType
            ? context.Request.Form
                .Where(v => !excluding.Contains(v.Key))
                .ToDictionary(v => v.Key, v => v.Value)
            : context.Request.Query
                .Where(v => !excluding.Contains(v.Key))
                .ToDictionary(v => v.Key, v => v.Value);

        return parameters;
    }

    public static string BuildRedirectUrl(HttpRequest request, IDictionary<string, StringValues> oAuthparameters)
    {
        var url = $"{request.PathBase}{request.Path}{QueryString.Create(oAuthparameters)}";
        return url;
    }

    public static bool IsAuthenticated(AuthenticateResult authenticateResult, OpenIddictRequest request)
    {
        if (!authenticateResult.Succeeded) return false;

        if (!request.MaxAge.HasValue || authenticateResult.Properties is null) return true;

        var maxAgeSeconds = TimeSpan.FromSeconds(request.MaxAge.Value);

        var expired = !authenticateResult.Properties.IssuedUtc.HasValue ||
                      DateTimeOffset.UtcNow - authenticateResult.Properties.IssuedUtc > maxAgeSeconds;

        return !expired;
    }

    public static List<string> GetDestinations(ClaimsIdentity identity, Claim claim)
    {
        List<string> destinations = [];

        if (claim.Type is OpenIddictConstants.Claims.Name or OpenIddictConstants.Claims.Email
            or OpenIddictConstants.Claims.Role)
            destinations.Add(OpenIddictConstants.Destinations.AccessToken);

        return destinations;
    }
}