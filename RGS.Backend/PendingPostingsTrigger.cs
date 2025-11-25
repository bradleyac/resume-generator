using Azure.AI.OpenAI;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RGS.Backend.Shared.Models;
using RGS.Backend.Services;

namespace RGS.Backend;

internal class PendingPostingsTrigger
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