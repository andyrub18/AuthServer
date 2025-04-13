using AuthServer.Data.Repository;
using AuthServer.Dtos;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthServer.Pages.Admin.Clients;

public class IndexModel(ClientAppRepository repository) : PageModel
{
    public IEnumerable<ClientSummaryDto> Clients { get; private set; } = null!;
    public string? Filter { get; set; }

    public async Task OnGetAsync(string? filter)
    {
        Filter = filter;
        Clients = await repository.GetClients();
    }
}