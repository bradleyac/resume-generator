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

public class GetResumeData(ILogger<GetResumeData> logger, CosmosClient cosmosClient)
{
  private readonly ILogger<GetResumeData> _logger = logger;
  private readonly CosmosClient _cosmosClient = cosmosClient;

  [Function("GetResumeData")]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
  {
    try
    {
      var postingId = req.Query["postingId"].FirstOrDefault() ?? throw new ArgumentException("postingId missing");
      var resumeDataContainer = _cosmosClient.GetContainer("Resumes", "ResumeData");
      var resumeData = await resumeDataContainer.ReadItemAsync<ResumeData>(postingId, new PartitionKey(postingId));

      return resumeData switch
      {
        null => new NotFoundResult(),
        ItemResponse<ResumeData> p => p switch
        {
          { StatusCode: System.Net.HttpStatusCode.OK } => new JsonResult(p.Resource),
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