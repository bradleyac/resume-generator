using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using RGS.Backend.Shared.Models;
using RGS.Backend.Shared;
using RGS.Backend.Services;
using RGS.Backend.Shared.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace RGS.Backend;

internal class SetResumeData(ILogger<SetResumeData> logger, CosmosClient cosmosClient, IUserService userService)
{
    private readonly ILogger<SetResumeData> _logger = logger;
    private readonly CosmosClient _cosmosClient = cosmosClient;
    private readonly IUserService _userService = userService;

    [Function("SetResumeData")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        try
        {
            var currentUserId = _userService.GetCurrentUserId();
            if (currentUserId is null)
            {
                return new UnauthorizedResult();
            }
            var payload = await req.ReadFromJsonAsync<ResumeData>() ?? throw new ArgumentException("Invalid payload");

            if (!Validator.TryValidateObject(payload, new ValidationContext(payload), []))
            {
                return new BadRequestResult();
            }

            // "master" for each user is stored as their userid.
            var postingId = payload.id == "master" ? currentUserId : payload.id;

            var resumeDataContainer = _cosmosClient.GetContainer("Resumes", "ResumeData");
            var existingData = await resumeDataContainer.ReadItemAsync<ResumeData>(payload.id, new PartitionKey(payload.id));

            if (existingData.StatusCode != System.Net.HttpStatusCode.OK || existingData.Resource.UserId != currentUserId)
            {
                return new NotFoundResult();
            }

            await resumeDataContainer.UpsertItemAsync(payload, new PartitionKey(payload.id));
            return new OkResult();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to set resume data");
            throw;
        }
    }
}