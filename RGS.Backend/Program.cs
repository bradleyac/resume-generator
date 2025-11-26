using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RGS.Backend.Services;
using RGS.Backend.Middleware;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddApplicationServices();

builder.UseMiddleware<FunctionContextAccessorMiddleware>();
builder.UseMiddleware<EasyAuthMiddleware>();

builder.Configuration.AddEnvironmentVariables();

builder.Build().Run();

public static class BuilderExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection @this)
    {
        var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");
        @this.AddSingleton(new CosmosClient(connectionString));

        var openAIEndpoint = Environment.GetEnvironmentVariable("AzureOpenAIEndpoint") ?? throw new InvalidOperationException("AzureOpenAIEndpoint not set");
        var openAIKey = Environment.GetEnvironmentVariable("AzureOpenAIKey") ?? throw new InvalidOperationException("AzureOpenAIKey not set");
        @this.AddTransient(typeof(AzureOpenAIClient), (_) => new AzureOpenAIClient(new Uri(openAIEndpoint), new AzureKeyCredential(openAIKey)));

        @this.AddTransient<PostingProcessor>();
        @this.AddScoped<UserService>();
        @this.AddScoped<FunctionContextAccessor>();

        return @this;
    }
}