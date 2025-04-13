using System.Text.Json;
using AuthServer.Dtos;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;

namespace AuthServer.Data.Repository;

public class ScopesRepository(IOpenIddictScopeManager scopesManager, ILogger<ScopesRepository> logger)
{
    public async Task<bool> AddScope(ScopeDto scope)
    {
        var apiScope = await scopesManager.FindByNameAsync(scope.Name);

        if (apiScope is not null) await scopesManager.DeleteAsync(apiScope);

        var scp = new OpenIddictScopeDescriptor
        {
            DisplayName = scope.DisplayName,
            Name = scope.Name,
            Description = scope.Description,
        };

        /*
         * For now, we have a resource by scope, let us set it by the name of the resource
         * TODO: if in the future I got multiple related microservices, use multiple resources for single scope instead
         */
        scp.Resources.Add(scope.Name);

        try
        {
            await scopesManager.CreateAsync(scp);
        }
        catch (Exception e)
        {
            logger.LogError(e, "The scopes could not be added because of the error {error}", e.Message);
            return false;
        }


        return true;
    }

    public async Task<List<ScopeSummaryDto>> GetScopes()
    {
        List<ScopeSummaryDto> scopes = [];
        var result = scopesManager.ListAsync();

        await foreach (var item in result)
        {
            var scope = (OpenIddictEntityFrameworkCoreScope)item;
            scopes.Add(
                new(
                    scope.Id!,
                    scope.Name!,
                    scope.DisplayName!,
                    scope.Description!
                )
            );
        }

        return scopes;
    }

    public async Task<EditScopeDto?> GetScope(string scopeId)
    {
        var apiScope = (OpenIddictEntityFrameworkCoreScope?)await scopesManager.FindByIdAsync(scopeId);

        if (apiScope is null) return null;

        return new(
            apiScope.Id!,
            apiScope.Name!,
            apiScope.DisplayName!,
            apiScope.Description!,
            JsonSerializer.Deserialize<HashSet<string>>(apiScope.Resources!) ?? []
        );
    }

    public async Task<bool> UpdateScope(EditScopeDto scope)
    {
        var result = (OpenIddictEntityFrameworkCoreScope?)await scopesManager.FindByIdAsync(scope.Id);

        if (result is null) return false;

        var newScope = new OpenIddictScopeDescriptor
        {
            Name = scope.Name,
            DisplayName = scope.DisplayName,
            Description = scope.Description,
        };

        /*
        // * For now, we have a resource by scope, let us set it by the name of the resource
        // * TODO: If in the future I got multiple related microservices, use multiple resources for single scope instead
        // */
        newScope.Resources.Add(scope.Name);

        try
        {
            await scopesManager.UpdateAsync(result, newScope);
        }
        catch (Exception e)
        {
            logger.LogInformation(e, "The scope could not be updated because of the error {message}", e.Message);
            return false;
        }

        return false;
    }
}