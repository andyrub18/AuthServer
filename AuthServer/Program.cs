using AuthServer.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddRazorPages();
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

var app = builder.Build();
app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseIdentityMiddlewarePipeline();

app.Run();