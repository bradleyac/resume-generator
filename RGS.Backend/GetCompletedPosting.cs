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

internal class GetCompletedPosting(ILogger<ListCompletedPostings> logger, IUserDataRepository userDataRepository)
{
  private readonly ILogger<ListCompletedPostings> _logger = logger;
  private readonly IUserDataRepository _userDataRepository = userDataRepository;

  [Function("GetCompletedPosting")]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
  {
    try
    {
      var completedPostingId = req.Query["completedPostingId"].FirstOrDefault() ?? throw new ArgumentException("Payload missing");
      var posting = await _userDataRepository.GetPostingAsync(completedPostingId);

      return posting switch
      {
        null => new NotFoundResult(),
        _ => new JsonResult(posting),
      };
    }
    catch (Exception e)
    {
      _logger.LogError(e, "Failed to load job posting");
      throw;
    }
  }
}