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

namespace RGS.Backend;

internal class GetSourceResumeData(ILogger<GetSourceResumeData> logger, IUserDataRepository userDataRepository)
{
    private readonly ILogger<GetSourceResumeData> _logger = logger;
    private readonly IUserDataRepository _userDataRepository = userDataRepository;

    [Function("GetSourceResumeData")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            return await _userDataRepository.GetSourceResumeDataAsync() switch
            {
                null => new NotFoundResult(),
                SourceResumeData resumeData => new JsonResult(resumeData),
            };

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load source resume data");
            throw;
        }
    }
}