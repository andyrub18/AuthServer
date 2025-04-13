using Microsoft.AspNetCore.Identity;

namespace AuthServer.Models;

public class ApplicationRole : IdentityRole<Guid>
{
    public string Description { get; set; } = string.Empty;
    public ApplicationRoleLevel Level { get; set; } = ApplicationRoleLevel.Customer;
}

public enum ApplicationRoleLevel
{
    Admin = 0,
    Manager = 1,
    Editor = 2,
    Viewer = 3,
    Customer = 4,
}