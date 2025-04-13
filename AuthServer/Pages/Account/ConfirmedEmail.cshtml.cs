using AuthServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthServer.Pages.Account;

public class ConfirmedEmail(UserManager<ApplicationUser> userManager, ILogger<ConfirmedEmail> logger) : PageModel
{
    [BindProperty] public string Message { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string userId, string token)
    {
        logger.LogInformation("The id is {userId} and the token is {token}", userId, token);

        var user = await userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            var result = await userManager.ConfirmEmailAsync(user, token);
            Message = result.Succeeded
                ? "Email address is successfully confirmed, you can now try to login."
                : "Failed to validate email address.";
            return Page();
        }

        Message = "User not found.";
        return Page();
    }
}