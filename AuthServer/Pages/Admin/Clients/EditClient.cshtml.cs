using AuthServer.Data.Repository;
using AuthServer.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AuthServer.Pages.Admin.Clients;

public class EditClient(ClientAppRepository repository, ScopesRepository scopesRepository)
    : PageModel
{
    [BindProperty] public EditClientDto InputModel { get; set; } = null!;
    public MultiSelectList? AvailableScopes { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        var model = await repository.GetClient(id);
        if (model is null) return RedirectToPage("/Admin/Clients/Index");
        var scopes = await scopesRepository.GetScopes();
        var clientScopes = model.AllowedScopes.Where(x =>
                x.StartsWith("scp") && !x.EndsWith("email") && !x.EndsWith("profile") && !x.EndsWith("roles"))
            .Select(x => x["scp:".Length..])
            .ToHashSet();
        InputModel = model with { AllowedScopes = clientScopes };
        AvailableScopes = new(scopes, nameof(ScopeSummaryDto.Name), nameof(ScopeSummaryDto.Description));
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        if (!ModelState.IsValid) return Page();
        var res = await repository.UpdateClient(InputModel);
        if (res) return RedirectToPage("/Admin/Clients/Index");
        return Page();
    }
}