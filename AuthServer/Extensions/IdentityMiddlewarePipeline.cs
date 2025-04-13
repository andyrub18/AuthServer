using AuthServer.Endpoints;

namespace AuthServer.Extensions;

public static class IdentityMiddlewarePipeline
{
    public static WebApplication UseIdentityMiddlewarePipeline(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapAuthorizationEndpoint();
        app.MapExternalCallbackEndpoint();

        app.MapStaticAssets();
        app.MapRazorPages()
            .WithStaticAssets();

        return app;
    }
}