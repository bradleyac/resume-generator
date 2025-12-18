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
using System.Runtime.Intrinsics.Arm;
using System.Net;

namespace RGS.Backend.Functions;

internal class GetSourceResumeData(ILogger<GetSourceResumeData> logger, IUserDataRepository userDataRepository)
{
    private readonly ILogger<GetSourceResumeData> _logger = logger;
    private readonly IUserDataRepository _userDataRepository = userDataRepository;

    [Function("GetSourceResumeData")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var result = await _userDataRepository.GetSourceResumeDataAsync();

        return result.ToJsonActionResult();
    }
}