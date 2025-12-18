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
using RGS.Backend;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddApplicationServices(builder.Environment.IsDevelopment());

builder.UseMiddleware<FunctionContextAccessorMiddleware>();
builder.UseMiddleware<EasyAuthMiddleware>();

builder.Configuration.AddEnvironmentVariables();

builder.Build().Run();

public static class BuilderExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection @this, bool isDevelopment)
    {
        var connectionString = Environment.GetEnvironmentVariable("CosmosDBConnectionString") ?? throw new RGSException("CosmosDBConnectionString not set");
        @this.AddSingleton(new CosmosClient(connectionString, new CosmosClientOptions { Serializer = new CosmosSystemTextJsonSerializer() }));

        var openAIEndpoint = Environment.GetEnvironmentVariable("AzureOpenAIEndpoint") ?? throw new RGSException("AzureOpenAIEndpoint not set");
        var openAIKey = Environment.GetEnvironmentVariable("AzureOpenAIKey") ?? throw new RGSException("AzureOpenAIKey not set");
        @this.AddTransient(typeof(AzureOpenAIClient), (_) => new AzureOpenAIClient(new Uri(openAIEndpoint), new AzureKeyCredential(openAIKey)));

        @this.AddTransient<PostingProcessor>();
        if (isDevelopment)
        {
            @this.AddScoped<IUserService, DevelopmentUserService>();
        }
        else
        {
            @this.AddScoped<IUserService, UserService>();
        }
        @this.AddScoped<FunctionContextAccessor>();
        @this.AddScoped<IUserDataRepositoryFactory, UserDataRepositoryFactory>();
        @this.AddScoped<IUserDataRepository, UserDataRepository>();

        return @this;
    }
}