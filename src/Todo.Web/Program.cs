using Microsoft.Extensions.Http.Resilience;
using Todo.Web;
using Todo.Web.Components;

var builder = WebApplication.CreateBuilder(args);

var apiBaseAddress = builder.Configuration["TodoApi:BaseAddress"]
    ?? throw new InvalidOperationException("Setting 'TodoApi:BaseAddress' is not configured.");

builder.Services.AddHttpClient<TodoApiClient>(client => client.BaseAddress = new Uri(apiBaseAddress))
    // O retry vem ligado para todos os métodos, POST incluído, e num POST isso duplica escritas.
    .AddStandardResilienceHandler(options => options.Retry.DisableForUnsafeHttpMethods());

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
