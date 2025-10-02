using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RGS.Backend.Shared.Models;

namespace RGS.Functions;

public class ImportJobPosting(ILogger<ImportJobPosting> logger, CosmosClient cosmosClient)
{
    private readonly ILogger<ImportJobPosting> _logger = logger;
    private readonly CosmosClient _cosmosClient = cosmosClient;

    [Function("ImportJobPosting")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        try
        {
            var pendingPostings = _cosmosClient.GetContainer("Resumes", "PendingPostings");
            var payload = await req.ReadFromJsonAsync<JobPostingPayload>() ?? throw new ArgumentException("Payload missing");
            await pendingPostings.UpsertItemAsync(new JobPosting(Guid.NewGuid().ToString(), payload.PostingText, DateTime.UtcNow));
            return new OkResult();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to import job posting");
            throw;
        }
    }
}

public record JobPostingPayload(string PostingText);