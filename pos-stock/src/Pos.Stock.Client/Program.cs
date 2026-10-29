using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Pos.Stock.Client;
using Pos.Stock.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// The API runs on its own origin. Its base URL comes from wwwroot/appsettings.json (Api:BaseUrl).
// Relative request paths such as "health" only resolve correctly when the base ends with "/".
var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Missing Api:BaseUrl in wwwroot/appsettings.json.");
var apiBase = new Uri(apiBaseUrl.TrimEnd('/') + "/", UriKind.Absolute);

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = apiBase });
builder.Services.AddScoped<ApiClient>();

await builder.Build().RunAsync();
