using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RGS.Backend.Shared.Models;

namespace RGS.Backend;

public class SetPostingStatus(ILogger<SetPostingStatus> logger, CosmosClient cosmosClient)
{
    private readonly ILogger<SetPostingStatus> _logger = logger;
    private readonly CosmosClient _cosmosClient = cosmosClient;


    [Function("SetPostingStatus")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.User, "post")] HttpRequest req)
    {
        try
        {
            // TODO: CSRF protection
            // Currently using cookie-based authentication built into Azure Functions, and thus vulnerable to CSRF.
            // Fixes include changing to token-based authentication in headers or implementing anti-CSRF tokens.

            var payload = await req.ReadFromJsonAsync<PostingStatusUpdate>() ?? throw new ArgumentException("Invalid payload");

            if (!PostingStatus.ValidStatuses.Contains(payload.NewStatus))
            {
                return new BadRequestObjectResult("Invalid status");
            }

            var postings = _cosmosClient.GetContainer("Resumes", "Postings");
            var postingResponse = await postings.ReadItemAsync<JobPosting>(payload.PostingId, new PartitionKey(payload.PostingId));

            if (postingResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                return new NotFoundResult();
            }

            var posting = postingResponse.Resource;

            if (posting is not null && posting.Status != payload.NewStatus)
            {
                posting = posting with { Status = payload.NewStatus };
                await postings.UpsertItemAsync(posting);
            }

            return new OkResult();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to import job posting");
            throw;
        }
    }
}