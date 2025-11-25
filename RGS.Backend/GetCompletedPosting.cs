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

internal class GetCompletedPosting(ILogger<ListCompletedPostings> logger, CosmosClient cosmosClient, UserService userService)
{
  private readonly ILogger<ListCompletedPostings> _logger = logger;
  private readonly CosmosClient _cosmosClient = cosmosClient;
  private readonly UserService _userService = userService;

  [Function("GetCompletedPosting")]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
  {
    try
    {
      var currentUser = _userService.GetCurrentUser();
      if (currentUser is null)
      {
        return new UnauthorizedResult();
      }

      var completedPostingId = req.Query["completedPostingId"].FirstOrDefault() ?? throw new ArgumentException("Payload missing");
      var completedPostingsContainer = _cosmosClient.GetContainer("Resumes", "Postings");
      var posting = await completedPostingsContainer.ReadItemAsync<JobPosting>(completedPostingId, new PartitionKey(completedPostingId));

      return posting switch
      {
        null => new NotFoundResult(),
        ItemResponse<JobPosting> p => p switch
        {
          { StatusCode: System.Net.HttpStatusCode.OK } => (p.Resource.UserId == currentUser.UserId) switch
          {
            true => new JsonResult(p.Resource),
            false => new NotFoundResult()
          },
          _ => new NotFoundResult()
        }
      };

    }
    catch (Exception e)
    {
      _logger.LogError(e, "Failed to load job posting");
      throw;
    }
  }
}