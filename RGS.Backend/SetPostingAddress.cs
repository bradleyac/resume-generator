using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RGS.Backend.Services;
using RGS.Backend.Shared.Models;

namespace RGS.Backend;

internal class SetPostingAddress(ILogger<SetPostingAddress> logger, IUserDataRepository userDataRepository)
{
    private readonly ILogger<SetPostingAddress> _logger = logger;
    private readonly IUserDataRepository _userDataRepository = userDataRepository;

    [Function("SetPostingAddress")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        try
        {
            // TODO: CSRF protection
            // Currently using cookie-based authentication built into Azure Functions, and thus vulnerable to CSRF.
            // Fixes include changing to token-based authentication in headers or implementing anti-CSRF tokens.
            var payload = await req.ReadFromJsonAsync<UpdatePostingAddressModel>() ?? throw new ArgumentException("Invalid payload");

            bool result = await _userDataRepository.SetPostingAddressAsync(payload);

            return result ? new OkResult() : new NotFoundResult();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update job posting address.");
            throw;
        }
    }
}