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
using System.Net;

namespace RGS.Backend.Functions;

internal class GetResumeData(ILogger<GetResumeData> logger, IUserDataRepository userDataRepository)
{
  private readonly ILogger<GetResumeData> _logger = logger;
  private readonly IUserDataRepository _userDataRepository = userDataRepository;

  [Function("GetResumeData")]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
  {
    var postingId = req.Query["postingId"].FirstOrDefault();
    if (postingId is null) return new BadRequestResult();

    var result = await _userDataRepository.GetResumeDataAsync(postingId);

    return result.ToJsonActionResult();
  }
}