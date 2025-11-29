using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using RGS.Backend.Services;
using RGS.Backend.Shared.Models;

namespace RGS.Backend;

internal class DeletePosting(ILogger<DeletePosting> logger, CosmosClient cosmosClient, IUserService userService)
{
    private readonly ILogger<DeletePosting> _logger = logger;
    private readonly CosmosClient _cosmosClient = cosmosClient;
    private readonly IUserService _userService = userService;

    [Function("DeletePosting")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeletePosting/{postingId}")] HttpRequest req, string postingId)
    {
        try
        {
            // TODO: CSRF protection
            // Currently using cookie-based authentication built into Azure Functions, and thus vulnerable to CSRF.
            // Fixes include changing to token-based authentication in headers or implementing anti-CSRF tokens.
            var currentUserId = _userService.GetCurrentUserId();
            if (currentUserId is null)
            {
                return new UnauthorizedResult();
            }

            var postings = _cosmosClient.GetContainer("Resumes", "Postings");

            var postingResult = await postings.ReadItemAsync<JobPosting>(postingId, new PartitionKey(postingId));

            // Ensure posting exits and current user owns it
            if (postingResult.StatusCode != System.Net.HttpStatusCode.OK || postingResult.Resource?.UserId != currentUserId)
            {
                return new NotFoundResult();
            }

            await postings.DeleteItemAsync<JobPosting>(postingId, new PartitionKey(postingId));

            var resumes = _cosmosClient.GetContainer("Resumes", "ResumeData");
            await resumes.DeleteItemAsync<ResumeData>(postingId, new PartitionKey(postingId));

            return new OkResult();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete job posting");
            throw;
        }
    }
}