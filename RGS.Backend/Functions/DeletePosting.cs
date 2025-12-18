using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using RGS.Backend.Services;
using RGS.Backend.Shared.Models;

namespace RGS.Backend.Functions;

internal class DeletePosting(ILogger<DeletePosting> logger, IUserDataRepository userDataRepository)
{
    private readonly ILogger<DeletePosting> _logger = logger;
    private readonly IUserDataRepository _userDataRepository = userDataRepository;

    [Function("DeletePosting")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "DeletePosting/{postingId}")] HttpRequest req, string postingId)
    {
        // TODO: CSRF protection
        // Currently using cookie-based authentication built into Azure Functions, and thus vulnerable to CSRF.
        // Fixes include changing to token-based authentication in headers or implementing anti-CSRF tokens.
        var result = await _userDataRepository.DeletePostingAsync(postingId);

        return result.ToActionResult();
    }
}