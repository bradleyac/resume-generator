using System.Collections.Specialized;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RGS.Backend.Shared.Models;

namespace RGS.Backend;

public class ImportJobPosting(ILogger<ImportJobPosting> logger, CosmosClient cosmosClient)
{
    private readonly ILogger<ImportJobPosting> _logger = logger;
    private readonly CosmosClient _cosmosClient = cosmosClient;

    [Function("ImportJobPosting")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        try
        {
            // todo: CSRF
            var pendingPostings = _cosmosClient.GetContainer("Resumes", "PendingPostings");
            var form = await req.ReadFormAsync();
            var newPosting = new JobPosting
            (
                Guid.NewGuid().ToString(),
                form.Get("link"),
                form.Get("company"),
                form.Get("title"),
                form.Get("postingText"),
                DateTime.UtcNow
            );
            await pendingPostings.UpsertItemAsync(newPosting);
            return new OkResult();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to import job posting");
            throw;
        }
    }
}

public static class ImportJobPostingExtensions
{

}