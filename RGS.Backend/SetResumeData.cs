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

public class SetResumeData(ILogger<SetResumeData> logger, CosmosClient cosmosClient)
{
  private readonly ILogger<SetResumeData> _logger = logger;
  private readonly CosmosClient _cosmosClient = cosmosClient;

  [Function("SetResumeData")]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
  {
    try
    {
        var payload = await req.ReadFromJsonAsync<ResumeData>() ?? throw new ArgumentException("Invalid payload");
        var resumeDataContainer = _cosmosClient.GetContainer("Resumes", "ResumeData");
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