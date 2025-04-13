using AuthServer.Data.Repository;
using AuthServer.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthServer.Pages.Admin.Scopes;

public class AddNewModel(ScopesRepository repository) : PageModel
{
    [BindProperty] public ScopeDto InputModel { get; set; } = null!;

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var result = await repository.AddScope(InputModel);

        if (result) return RedirectToPage("/Admin/Scopes/Index");

        return Page();
    }
}