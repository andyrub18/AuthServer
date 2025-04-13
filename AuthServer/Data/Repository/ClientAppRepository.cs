using AuthServer.Dtos;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using static OpenIddict.Abstractions.OpenIddictConstants;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AuthServer.Data.Repository;

public class ClientAppRepository(IOpenIddictApplicationManager applicationManager, ILogger<ClientAppRepository> logger)
{
    public bool CreateClient(ClientDto client)
    {
        var application = new OpenIddictApplicationDescriptor
        {
            ClientId = client.ClientId,
            ClientSecret = client.ClientSecret,
            ClientType = ClientTypes.Public,
            ConsentType = ConsentTypes.Explicit,
            DisplayName = client.DisplayName,
            RedirectUris = { new(client.RedirectUri) },
            PostLogoutRedirectUris = { new(client.PostLogoutRedirectUris) },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.EndSession,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.Revocation,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
            },
        };

        foreach (var scope in client.AllowedScopes) application.Permissions.Add($"{Permissions.Prefixes.Scope}{scope}");

        var res = applicationManager.CreateAsync(application);

        return res.IsCompletedSuccessfully;
    }

    public async Task<List<ClientSummaryDto>> GetClients()
    {
        List<ClientSummaryDto> clients = [];
        var result = applicationManager.ListAsync();

        await foreach (var client in result)
        {
            var clientApp = (OpenIddictEntityFrameworkCoreApplication)client;
            clients.Add(
                new(
                    clientApp.Id!,
                    clientApp.ClientId!,
                    clientApp.DisplayName!
                )
            );
        }

        return clients;
    }

    public async Task<EditClientDto?> GetClient(string clientId)
    {
        var result = (OpenIddictEntityFrameworkCoreApplication?)await applicationManager.FindByIdAsync(clientId);

        if (result is null) return null;

        /*
         * TODO: If I got one app that have multiple redirect uris or multiple post logout uris use hashset instead of simple string
         */

        return new(
            result.Id!,
            result.ClientId!,
            result.DisplayName!,
            JsonSerializer.Deserialize<HashSet<string>>(result.RedirectUris!)?.First() ?? "",
            JsonSerializer.Deserialize<HashSet<string>>(result.PostLogoutRedirectUris!)?.First() ?? "",
            JsonSerializer.Deserialize<HashSet<string>>(result.Permissions!) ?? []
        );
    }

    public async Task<bool> UpdateClient(EditClientDto client)
    {
        var application = await applicationManager.FindByIdAsync(client.Id);

        if (application is null) return false;

        var app = new OpenIddictApplicationDescriptor
        {
            ClientId = client.ClientId,
            ClientType = ClientTypes.Public,
            ClientSecret = application.GetType().GetProperty("ClientSecret")?.GetValue(application)?.ToString(),
            RedirectUris = { new(client.RedirectUri) },
            DisplayName = client.DisplayName,
            PostLogoutRedirectUris = { new(client.PostLogoutRedirectUris) },
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.EndSession,
                Permissions.Endpoints.Token,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Email,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Roles,
            },
        };

        foreach (var scope in client.AllowedScopes) app.Permissions.Add($"{Permissions.Prefixes.Scope}{scope}");

        try
        {
            await applicationManager.UpdateAsync(application, app);
        }
        catch (Exception e)
        {
            logger.LogError(e, "The application couldn't be updated because of the error {error}", e.Message);
            return false;
        }

        return true;
    }
}