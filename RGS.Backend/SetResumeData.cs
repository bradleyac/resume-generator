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
using System.Net;

namespace RGS.Backend;

internal class SetResumeData(ILogger<SetResumeData> logger, IUserDataRepository userDataRepository)
{
    private readonly ILogger<SetResumeData> _logger = logger;
    private readonly IUserDataRepository _userDataRepository = userDataRepository;

    [Function("SetResumeData")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        var payload = await req.ReadFromJsonAsync<ResumeData>();
        if (payload is null || !Validator.TryValidateObject(payload, new ValidationContext(payload), []))
        {
            return new BadRequestResult();
        }

        var result = await _userDataRepository.SetResumeDataAsync(payload);

        return result switch
        {
            { IsSuccess: true } => new OkResult(),
            { IsSuccess: false, StatusCode: HttpStatusCode statusCode } => new StatusCodeResult((int)statusCode),
            _ => new StatusCodeResult((int)HttpStatusCode.InternalServerError),
        };
    }
}