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
        try
        {
            // TODO: CSRF protection
            // Currently using cookie-based authentication built into Azure Functions, and thus vulnerable to CSRF.
            // Fixes include changing to token-based authentication in headers or implementing anti-CSRF tokens.

            var payload = await req.ReadFromJsonAsync<PostingStatusUpdate>() ?? throw new ArgumentException("Invalid payload");

            if (!PostingStatus.ValidStatuses.Contains(payload.NewStatus))
            {
                return new BadRequestObjectResult("Invalid status");
            }

            bool result = await _userDataRepository.SetPostingStatusAsync(payload.PostingId, payload.NewStatus);

            return result ? new OkResult() : new NotFoundResult();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to import job posting");
            throw;
        }
    }
}