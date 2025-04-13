using AuthServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthServer.Pages.Admin.Users;

public class UsersModel
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

[Authorize(Roles = "Administrator, Manager, Editor, Viewer")]
public class IndexModel(UserManager<ApplicationUser> userManager) : PageModel
{
    public IEnumerable<UsersModel> Users { get; private set; } = null!;

    public void OnGet()
    {
        Users = userManager.Users.Select(e => new UsersModel
        {
            FirstName = e.FirstName!,
            LastName = e.LastName!,
            UserName = e.UserName!,
            Email = e.Email!,
        }).ToList();
    }
}