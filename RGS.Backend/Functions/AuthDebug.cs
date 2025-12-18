using System.Security.Claims;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RGS.Backend.Middleware;

namespace RGS.Backend.Functions;

public class AuthDebug
{
    private readonly ILogger<AuthDebug> _logger;

    public AuthDebug(ILogger<AuthDebug> logger)
    {
        _logger = logger;
    }

    [Function("AuthDebug")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req, FunctionContext context)
    {
        var user = context.Items["User"] as EasyAuthUser;
        var headers = new Dictionary<string, string>
        {
            { "X-MS-CLIENT-PRINCIPAL", req.Headers["X-MS-CLIENT-PRINCIPAL"].ToString() },
            { "X-MS-CLIENT-PRINCIPAL-ID", req.Headers["X-MS-CLIENT-PRINCIPAL-ID"].ToString() },
            { "X-MS-CLIENT-PRINCIPAL-IDP", req.Headers["X-MS-CLIENT-PRINCIPAL-IDP"].ToString() },
            { "X-MS-CLIENT-PRINCIPAL-NAME", req.Headers["X-MS-CLIENT-PRINCIPAL-NAME"].ToString() },
        };

        return new OkObjectResult(headers);
    }
}