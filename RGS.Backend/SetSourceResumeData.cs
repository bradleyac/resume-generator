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
using RGS.Backend.Shared.ViewModels;
using System.ComponentModel.DataAnnotations;
using Grpc.Core;
using System.Net;

namespace RGS.Backend;

internal class SetSourceResumeData(ILogger<SetResumeData> logger, IUserDataRepository userDataRepository)
{
    private readonly ILogger<SetResumeData> _logger = logger;
    private readonly IUserDataRepository _userDataRepository = userDataRepository;

    [Function("SetSourceResumeData")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        try
        {
            var payload = await req.ReadFromJsonAsync<SourceResumeData>() ?? throw new ArgumentException("Invalid payload");

            if (!Validator.TryValidateObject(payload, new ValidationContext(payload), []))
            {
                return new BadRequestResult();
            }

            bool result = await _userDataRepository.SetSourceResumeDataAsync(payload);

            return result ? new OkResult() : new NotFoundResult();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to set source resume data");
            throw;
        }
    }
}