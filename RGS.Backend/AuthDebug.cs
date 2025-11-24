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
        var headers = new Dictionary<string, string>
        {
            { "X-MS-CLIENT-PRINCIPAL", req.Headers["X-MS-CLIENT-PRINCIPAL"].ToString() },
            { "X-MS-CLIENT-PRINCIPAL-ID", req.Headers["X-MS-CLIENT-PRINCIPAL-ID"].ToString() },
            { "X-MS-CLIENT-PRINCIPAL-IDP", req.Headers["X-MS-CLIENT-PRINCIPAL-IDP"].ToString() },
            { "X-MS-CLIENT-PRINCIPAL-NAME", req.Headers["X-MS-CLIENT-PRINCIPAL-NAME"].ToString() },
            { "IsAuthenticated", req.HttpContext.User.Identity?.IsAuthenticated.ToString() ?? "false"},
            { "UserName", req.HttpContext.User.Identity?.Name ?? "null" }
        };

        return new OkObjectResult(headers);
    }
}