using FluentValidation;
using Fluxor;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using R3;
using RGS.Backend.Shared.ViewModels;
using RGS.Frontend;
using RGS.Frontend.Store.EditResumeDataFeature;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

string apiBaseAddress = builder.Configuration["Api:BaseAddress"] ?? throw new RGSException("ApiBaseAddress not configured");

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });
builder.Services.AddSingleton<IPostingsService, PostingsService>();
builder.Services.AddSingleton<IResumeDataService, ResumeDataService>();
builder.Services.AddSingleton<Effects>();
builder.Services.AddBlazorBootstrap();
builder.Services.AddBlazorWebAssemblyR3();
builder.Services.AddFluxor(configureFluxor =>
    configureFluxor.ScanAssemblies(typeof(Program).Assembly));

await builder.Build().RunAsync();
