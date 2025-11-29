using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RGS.Backend.Services;
using RGS.Backend.Shared.Models;

namespace RGS.Backend;

internal class SetPostingAddress(ILogger<SetPostingStatus> logger, CosmosClient cosmosClient, IUserService userService)
{
    private readonly ILogger<SetPostingStatus> _logger = logger;
    private readonly CosmosClient _cosmosClient = cosmosClient;
    private readonly IUserService _userService = userService;

    [Function("SetPostingAddress")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        try
        {
            // TODO: CSRF protection
            // Currently using cookie-based authentication built into Azure Functions, and thus vulnerable to CSRF.
            // Fixes include changing to token-based authentication in headers or implementing anti-CSRF tokens.

            var payload = await req.ReadFromJsonAsync<UpdatePostingAddressModel>() ?? throw new ArgumentException("Invalid payload");

            var postings = _cosmosClient.GetContainer("Resumes", "Postings");
            var postingResponse = await postings.ReadItemAsync<JobPosting>(payload.PostingId, new PartitionKey(payload.PostingId));
            var currentUserId = _userService.GetCurrentUserId();

            if (postingResponse.StatusCode != System.Net.HttpStatusCode.OK || postingResponse.Resource.UserId != currentUserId)
            {
                return new NotFoundResult();
            }

            var posting = postingResponse.Resource;

            if (posting is not null)
            {
                posting = posting with { StreetAddress = payload.StreetAddress, City = payload.City, State = payload.State, Zip = payload.Zip, Status = PostingStatus.Pending };
                await postings.UpsertItemAsync(posting);
            }

            return new OkResult();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update job posting address.");
            throw;
        }
    }
}