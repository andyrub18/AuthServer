using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuthServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthServer.Pages.Account;

public class Login(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager
) : PageModel
{
    [BindProperty] public LoginModel LoginModel { get; set; } = new();

    [BindProperty] public string? ReturnUrl { get; set; }

    [BindProperty] public IList<AuthenticationScheme> ExternalLoginProviders { get; set; } = [];


    public async Task<IActionResult> OnGetAsync(string returnUrl = "/")
    {
        ReturnUrl = returnUrl;
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
        ExternalLoginProviders = [.. await signInManager.GetExternalAuthenticationSchemesAsync()];
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLoginProviders = [.. await signInManager.GetExternalAuthenticationSchemesAsync()];

        if (!ModelState.IsValid) return Page();

        var user = await userManager.FindByEmailAsync(LoginModel.Email);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, $"User with email {LoginModel.Email} not found");
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(user, LoginModel.Password, LoginModel.RememberMe, false);

        if (result.Succeeded)
        {
            List<Claim> claims = [new(ClaimTypes.Email, LoginModel.Email)];

            var principal = new ClaimsPrincipal(
                [new(claims, CookieAuthenticationDefaults.AuthenticationScheme)]
            );

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal
            );

            return Redirect(returnUrl);
        }

        if (result.RequiresTwoFactor)
            return RedirectToPage(
                "/Account/LoginTwoFactorWithAuthenticator",
                new { LoginModel.RememberMe }
            );

        ModelState.AddModelError("Login", result.IsLockedOut ? "You are locked out." : "Failed to login.");

        return Page();
    }
}

public class LoginModel
{
    [Required] public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember Me")] public bool RememberMe { get; set; }
}