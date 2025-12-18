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

internal class GetCompletedPosting(ILogger<GetCompletedPosting> logger, IUserDataRepository userDataRepository)
{
  private readonly ILogger<GetCompletedPosting> _logger = logger;
  private readonly IUserDataRepository _userDataRepository = userDataRepository;

  [Function("GetCompletedPosting")]
  public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
  {
    var completedPostingId = req.Query["completedPostingId"].FirstOrDefault();
    if (completedPostingId is null) return new BadRequestResult();

    var result = await _userDataRepository.GetPostingAsync(completedPostingId);

    return result.ToJsonActionResult();
  }
}