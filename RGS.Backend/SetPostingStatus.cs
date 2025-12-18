using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RGS.Backend.Services;
using RGS.Backend.Shared.Models;

namespace RGS.Backend;

internal class SetPostingStatus(ILogger<SetPostingStatus> logger, IUserDataRepository userDataRepository)
{
    private readonly ILogger<SetPostingStatus> _logger = logger;
    private readonly IUserDataRepository _userDataRepository = userDataRepository;


    [Function("SetPostingStatus")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        // TODO: CSRF protection
        // Currently using cookie-based authentication built into Azure Functions, and thus vulnerable to CSRF.
        // Fixes include changing to token-based authentication in headers or implementing anti-CSRF tokens.

        var payload = await req.ReadFromJsonAsync<PostingStatusUpdate>();

        if (payload is null || !PostingStatus.ValidStatuses.Contains(payload.NewStatus))
        {
            return new BadRequestResult();
        }

        var result = await _userDataRepository.SetPostingStatusAsync(payload.PostingId, payload.NewStatus);

        return result switch
        {
            { IsSuccess: true } => new OkResult(),
            { IsSuccess: false, StatusCode: HttpStatusCode statusCode } => new StatusCodeResult((int)statusCode),
            _ => new StatusCodeResult((int)HttpStatusCode.InternalServerError),
        };
    }
}