namespace AuthServer.Dtos;

public record ClientDto(
    string ClientId,
    string ClientSecret,
    string DisplayName,
    string RedirectUri,
    string PostLogoutRedirectUris,
    HashSet<string> AllowedScopes
);

public record ClientSummaryDto(
    string Id,
    string ClientId,
    string DisplayName
);

public record EditClientDto(
    string Id,
    string ClientId,
    string DisplayName,
    string RedirectUri,
    string PostLogoutRedirectUris,
    HashSet<string> AllowedScopes
);