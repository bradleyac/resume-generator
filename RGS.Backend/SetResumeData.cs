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

internal class SetResumeData(ILogger<SetResumeData> logger, CosmosClient cosmosClient, UserService userService)
{
    private readonly ILogger<SetResumeData> _logger = logger;
    private readonly CosmosClient _cosmosClient = cosmosClient;
    private readonly UserService _userService = userService;

    [Function("SetResumeData")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        try
        {
            // TODO: For master resume, use current user ID
            var currentUserId = _userService.GetCurrentUserId();
            if (currentUserId is null)
            {
                return new UnauthorizedResult();
            }
            var payload = await req.ReadFromJsonAsync<ResumeDataModel>() ?? throw new ArgumentException("Invalid payload");

            if (!Validator.TryValidateObject(payload, new ValidationContext(payload), []))
            {
                return new BadRequestResult();
            }

            var postingId = payload.id == "master" ? currentUserId : payload.id;

            var resumeDataContainer = _cosmosClient.GetContainer("Resumes", "ResumeData");
            var existingData = await resumeDataContainer.ReadItemAsync<ResumeData>(payload.id, new PartitionKey(payload.id));

            if (existingData.StatusCode != System.Net.HttpStatusCode.OK || existingData.Resource.UserId != currentUserId)
            {
                return new NotFoundResult();
            }

            var newData = existingData.Resource with
            {
                About = payload.About!,
                City = payload.City!,
                Name = payload.Name!,
                State = payload.State!,
                StreetAddress = payload.StreetAddress!,
                Title = payload.Title!,
                Zip = payload.Zip!,
                Contact = payload.Contact!.ValidatedUnrawp(),
                Bookshelf = [.. payload.Bookshelf.Select(b => b.ValidatedUnwrap())],
                Education = [.. payload.Education.Select(e => e.ValidatedUnwrap())],
                Jobs = [.. payload.Jobs.Select(j => j.ValidatedUnwrap())],
                Projects = [.. payload.Projects.Select(p => p.ValidatedUnwrap())],
                Skills = [.. payload.Skills.Select(s => s.ValidatedUnwrap())],
            };

            await resumeDataContainer.UpsertItemAsync(newData, new PartitionKey(newData.id));
            return new OkResult();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to set resume data");
            throw;
        }
    }
}