using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace RGS.Backend;

public class AuthDebug
{
    private readonly ILogger<AuthDebug> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthDebug(ILogger<AuthDebug> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    [Function("AuthDebug")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var result = new
        {
            IsAuthenticated = _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false,
            Name = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "N/A",
        };
        return new OkObjectResult(result);
    }
}