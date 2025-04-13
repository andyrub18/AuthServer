using AuthServer.Data.Repository;
using AuthServer.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthServer.Pages.Admin.Scopes;

public class EditModel(ScopesRepository repository) : PageModel
{
    [BindProperty] public EditScopeDto InputModel { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var model = await repository.GetScope(id);

        if (model is null) return RedirectToPage("/Admin/Scopes/Index");

        InputModel = model;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        if (!ModelState.IsValid) return Page();

        var res = await repository.UpdateScope(InputModel);
        if (res) return RedirectToPage("/Admin/Scopes/Index");

        return Page();
    }
}