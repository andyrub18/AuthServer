namespace AuthServer.Dtos;

public record ScopeDto(
    string Name,
    string DisplayName,
    string Description,
    HashSet<string>? Resources = null
);

public record ScopeSummaryDto(
    string Id,
    string Name,
    string DisplayName,
    string Description
);

public record EditScopeDto(
    string Id,
    string Name,
    string DisplayName,
    string Description,
    HashSet<string>? Resources = null
);