using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using AuthServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AuthServer.Pages.Account;

public class Register(
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager,
    IEmailSender emailSender
) : PageModel
{
    public MultiSelectList? AvailableRoles { get; set; }

    [BindProperty] public RegisterModel Model { get; set; } = new();

    public IActionResult OnGet()
    {
        PopulateRoles();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // create the user
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            FirstName = Model.FirstName,
            LastName = Model.LastName,
            UserName = Model.UserName,
            NormalizedUserName = Model.UserName.ToUpper(),
            Email = Model.Email,
            NormalizedEmail = Model.Email.ToUpper(),
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(user, Model.Password);

        // if the user is successfully created
        if (result.Succeeded)
        {
            // Add roles to the newly created user
            if (Model.Roles.Count == 0)
                await userManager.AddToRoleAsync(user, "Customer");
            else
                await userManager.AddToRolesAsync(user, Model.Roles);

            // trigger the email confirmation flow
            var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.PageLink("/Account/ConfirmedEmail",
                values: new { userId = user.Id, token = confirmationToken }) ?? string.Empty;

            await emailSender.SendEmailAsync(user.Email, "Please confirm your email",
                $"Please click on this link to confirm your email address: {confirmationLink}");

            return RedirectToPage("/Account/Login");
        }

        foreach (var error in result.Errors) ModelState.AddModelError("Register", error.Description);

        return Page();
    }

    private void PopulateRoles()
    {
        // Get the user with the role principal
        var user = userManager.GetUserAsync(User).Result;

        // If there's no user connected, don't choose role for the user
        // It's a user by default
        if (user is null) return;

        // Get the roles of the user
        var userRoles = userManager.GetRolesAsync(user).Result;

        // Get all the roles
        var maxRoleLevel = roleManager.Roles
            .Where(r => userRoles.Contains(r.Name!))
            .Select(r => r.Level)
            .Max();

        // If it's a simple user, don't give him the choice to choose other level of roles
        if (maxRoleLevel == ApplicationRoleLevel.Customer) return;

        // Get the roles to show according to the maximum level of the connected user
        var rolesToShow = roleManager.Roles
            .Where(r => r.Level >= maxRoleLevel)
            .OrderBy(x => x.Level)
            .ToImmutableList();

        // Create the selectList
        AvailableRoles = new(rolesToShow, nameof(ApplicationRole.Name), nameof(ApplicationRole.Name));
    }
}

public class RegisterModel
{
    [Required] public string FirstName { get; set; } = string.Empty;

    [Required] public string LastName { get; set; } = string.Empty;

    [Required]
    [MinLength(6, ErrorMessage = "The {0} must be at least {1} characters long.")]
    [MaxLength(20, ErrorMessage = "The {0} must be at most {1} characters long.")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = [];
}