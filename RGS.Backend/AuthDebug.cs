using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace RGS.Backend;

public class AuthDebug
{
    private readonly ILogger<AuthDebug> _logger;

    public AuthDebug(ILogger<AuthDebug> logger)
    {
        _logger = logger;
    }

    [Function("AuthDebug")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var result = new
        {
            IsAuthenticated = req.HttpContext?.User?.Identity?.IsAuthenticated ?? false,
            Name = req.HttpContext?.User?.Identity?.Name ?? "N/A",
        };
        return new OkObjectResult(result);
    }
}