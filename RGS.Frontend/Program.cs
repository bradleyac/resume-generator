using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using RGS.Frontend;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

string apiBaseAddress = builder.Configuration["Api:BaseAddress"] ?? throw new RGSException("ApiBaseAddress not configured");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });
builder.Services.AddScoped<IPostingsService, PostingsService>();
builder.Services.AddBlazorBootstrap();

await builder.Build().RunAsync();
