using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using RGS.Backend.Shared.Models;
using RGS.Backend.Shared;

namespace RGS.Functions;

public class GetCompletedPosting(ILogger<ListCompletedPostings> logger, CosmosClient cosmosClient)
{
  private readonly ILogger<ListCompletedPostings> _logger = logger;
  private readonly CosmosClient _cosmosClient = cosmosClient;

  [Function("GetCompletedPosting")]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
  {
    try
    {
      var payload = await req.ReadFromJsonAsync<GetCompletedPostingPayload>() ?? throw new ArgumentException("Payload missing");
      var completedPostingsContainer = _cosmosClient.GetContainer("Resumes", "CompletedPostings");
      var posting = await completedPostingsContainer.ReadItemAsync<CompletedPosting>(payload.id, new PartitionKey(payload.id));

      return posting switch
      {
        null => new NotFoundResult(),
        var p => new JsonResult(p)
      };

    }
    catch (Exception e)
    {
      _logger.LogError(e, "Failed to load job posting");
      throw;
    }
  }
}

public record GetCompletedPostingPayload(string id);