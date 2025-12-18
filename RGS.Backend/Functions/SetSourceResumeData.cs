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
using Microsoft.AspNetCore.Http.HttpResults;

namespace RGS.Backend.Functions;

internal class SetSourceResumeData(ILogger<SetSourceResumeData> logger, IUserDataRepository userDataRepository)
{
    private readonly ILogger<SetSourceResumeData> _logger = logger;
    private readonly IUserDataRepository _userDataRepository = userDataRepository;

    [Function("SetSourceResumeData")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        var payload = await req.ReadFromJsonAsync<SourceResumeData>();
        if (payload is null || !Validator.TryValidateObject(payload, new ValidationContext(payload), []))
        {
            return new BadRequestResult();
        }

        var result = await _userDataRepository.SetSourceResumeDataAsync(payload);

        return result.ToActionResult();
    }
}