using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using TodoApp.Client;
using TodoApp.Client.Gamification;
using TodoApp.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton<GamificationService>();
builder.Services.AddSingleton<AuditLogService>();
builder.Services.AddSingleton<TodoStore>();
builder.Services.AddSingleton<PersistenceService>();

// The API is a set of Vercel serverless functions under /api, deployed alongside
// this static site on the same domain - so same-origin is the right default. Only
// wwwroot/appsettings.Development.json overrides it, to point at a local functions
// dev server (e.g. `vercel dev`) while iterating with `dotnet run`.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
var baseAddress = string.IsNullOrWhiteSpace(apiBaseUrl) ? builder.HostEnvironment.BaseAddress : apiBaseUrl;
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(baseAddress) });
builder.Services.AddScoped<TodoApiClient>();

await builder.Build().RunAsync();
