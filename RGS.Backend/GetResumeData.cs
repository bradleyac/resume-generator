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

namespace RGS.Backend;

internal class GetResumeData(ILogger<GetResumeData> logger, CosmosClient cosmosClient, IUserService userService)
{
  private readonly ILogger<GetResumeData> _logger = logger;
  private readonly CosmosClient _cosmosClient = cosmosClient;
  private readonly IUserService _userService = userService;

  [Function("GetResumeData")]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
  {
    try
    {
      var currentUserId = _userService.GetCurrentUserId();
      if (currentUserId is null)
      {
        return new UnauthorizedResult();
      }

      var postingId = req.Query["postingId"].FirstOrDefault() ?? throw new ArgumentException("postingId missing");
      var resumeDataContainer = _cosmosClient.GetContainer("Resumes", "ResumeData");

      if (postingId == "master")
      {
        postingId = currentUserId;
      }

      var resumeData = await resumeDataContainer.ReadItemAsync<ResumeData>(postingId, new PartitionKey(postingId));

      return resumeData switch
      {
        null => new NotFoundResult(),
        ItemResponse<ResumeData> p => p switch
        {
          { StatusCode: System.Net.HttpStatusCode.OK } => (p.Resource.UserId == currentUserId) switch
          {
            true => new JsonResult(p.Resource.Wrap()),
            false => new NotFoundResult()
          },
          _ => new NotFoundResult()
        }
      };

    }
    catch (Exception e)
    {
      _logger.LogError(e, "Failed to load resume data");
      throw;
    }
  }
}