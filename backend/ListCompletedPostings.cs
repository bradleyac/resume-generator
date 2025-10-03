using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using RGS.Backend.Shared.Models;
using RGS.Backend.Shared;

namespace RGS.Backend;

public class ListCompletedPostings(ILogger<ListCompletedPostings> logger, CosmosClient cosmosClient)
{
    private readonly ILogger<ListCompletedPostings> _logger = logger;
    private readonly CosmosClient _cosmosClient = cosmosClient;

    [Function("ListCompletedPostings")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            var completedPostingsContainer = _cosmosClient.GetContainer("Resumes", "CompletedPostings");
            var query = completedPostingsContainer.GetItemLinqQueryable<CompletedPosting>()
                .Select(p => new { p.id, p.ImportedAt })
                .OrderByDescending(p => p.ImportedAt);
            var results = await query.ToFeedIterator().ToListAsync();
            return new JsonResult(results);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to list job postings");
            throw;
        }
    }
}