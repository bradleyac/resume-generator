using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .ConfigureServices();

builder.Build().Run();

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServices(this IServiceCollection @this)
    {
        var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString");
        @this.AddSingleton(new CosmosClient(connectionString));

        var openAIEndpoint = Environment.GetEnvironmentVariable("AzureOpenAIEndpoint");
        return @this.AddTransient(typeof(AzureOpenAIClient), (_) => new AzureOpenAIClient(new Uri(openAIEndpoint), new DefaultAzureCredential()));
    }
}