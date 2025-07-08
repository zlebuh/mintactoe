using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Zlebuh.MinTacToe.UI;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidDataException("Missing api base url in appsettings.json.");

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(apiBaseUrl) }
);
await builder.Build().RunAsync();
