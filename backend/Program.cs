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
    .AddApplicationServices();

builder.Build().Run();

public static class BuilderExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection @this)
    {
        return @this.AddSingleton(new CosmosClient(accountEndpoint: "https://resume-generation-system.documents.azure.com:443/", tokenCredential: new DefaultAzureCredential()));
    }
}