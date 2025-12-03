using FluentValidation;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using R3;
using RGS.Backend.Shared.ViewModels;
using RGS.Frontend;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

string apiBaseAddress = builder.Configuration["Api:BaseAddress"] ?? throw new RGSException("ApiBaseAddress not configured");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });
builder.Services.AddScoped<IPostingsService, PostingsService>();
builder.Services.AddScoped<IResumeDataService, ResumeDataService>();
builder.Services.AddBlazorBootstrap();
builder.Services.AddBlazorWebAssemblyR3();
builder.Services.AddSingleton<IValidator<ResumeDataModel>, ResumeDataValidator>();

await builder.Build().RunAsync();
