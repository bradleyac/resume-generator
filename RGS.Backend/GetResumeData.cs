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

internal class GetResumeData(ILogger<GetResumeData> logger, IUserDataRepository userDataRepository)
{
  private readonly ILogger<GetResumeData> _logger = logger;
  private readonly IUserDataRepository _userDataRepository = userDataRepository;

  [Function("GetResumeData")]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
  {
    try
    {
      var postingId = req.Query["postingId"].FirstOrDefault() ?? throw new ArgumentException("postingId missing");
      var resumeData = await _userDataRepository.GetResumeDataAsync(postingId);

      return resumeData switch
      {
        null => new NotFoundResult(),
        _ => new JsonResult(resumeData),
      };
    }
    catch (Exception e)
    {
      _logger.LogError(e, "Failed to load resume data");
      throw;
    }
  }
}