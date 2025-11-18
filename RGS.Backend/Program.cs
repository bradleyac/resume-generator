using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddApplicationServices();

builder.Configuration.AddEnvironmentVariables();

builder.Build().Run();

public static class BuilderExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection @this)
    {
        var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");
        @this.AddSingleton(new CosmosClient(connectionString));

        var openAIEndpoint = Environment.GetEnvironmentVariable("AzureOpenAIEndpoint");
        var openAIKey = Environment.GetEnvironmentVariable("AzureOpenAIKey");
        @this.AddTransient(typeof(AzureOpenAIClient), (_) => new AzureOpenAIClient(new Uri(openAIEndpoint), new AzureKeyCredential(openAIKey)));

        return @this;
    }
}