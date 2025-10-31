using System.Text.Json;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Newtonsoft.Json.Schema.Generation;
using OpenAI.Chat;
using RGS.Backend.Shared.Models;
using RGS.Backend.Shared;
using container.Services;

namespace RGS.Backend;

public class PendingPostingsTrigger
{
    private readonly ILogger<PendingPostingsTrigger> _logger;
    private readonly PostingProcessor _postingProcessor;

    public PendingPostingsTrigger(ILogger<PendingPostingsTrigger> logger, CosmosClient cosmosClient, AzureOpenAIClient aiClient, PostingProcessor postingProcessor)
    {
        _logger = logger;
        _postingProcessor = postingProcessor;
    }

    [Function("PendingPostingsTrigger")]
    public async Task Run([CosmosDBTrigger(
        databaseName: "Resumes",
        containerName: "Postings",
        Connection = "CosmosDBConnectionString",
        LeaseContainerName = "leases",
        CreateLeaseContainerIfNotExists = true)] IReadOnlyList<JobPosting> input)
    {
        foreach (var posting in input.Where(p => p.Status == PostingStatus.Pending))
        {
            await _postingProcessor.ProcessPendingPosting(posting);
        }
    }
}