using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Zlebuh.MinTacToe.GameSerialization;
using Zlebuh.MinTacToe.UI;
using Zlebuh.MinTacToe.UI.Services;

WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

string apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidDataException("Missing api base url in appsettings.json.");

string supabaseUrl = builder.Configuration["Supabase:Url"]
    ?? throw new InvalidDataException("Missing Supabase URL in appsettings.json.");

string supabaseKey = builder.Configuration["Supabase:SubscriptionKey"]
    ?? throw new InvalidDataException("Missing Supabase Key in appsettings.json.");

builder.Services.AddScoped<ISupabaseRealtime, SupabaseRealtime>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<ICookieService, CookieService>();
builder.Services.AddSingleton<ISerializer, GameJsonSerializer>();

await builder.Build().RunAsync();
