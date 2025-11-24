using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace RGS.Backend.Middleware;

internal class EasyAuthMiddleware : IFunctionsWorkerMiddleware
{
  private readonly FunctionExecutionDelegate _next;
  private readonly ILogger<EasyAuthMiddleware> _logger;

  public EasyAuthMiddleware(FunctionExecutionDelegate next, ILogger<EasyAuthMiddleware> logger)
  {
    _next = next;
    _logger = logger;
  }

  public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
  {
    // Extract user information from Easy Auth headers
    var req = await context.GetHttpRequestDataAsync();

    var principalHeader = req.Headers.Single(kvp => kvp.Key == "X-MS-CLIENT-PRINCIPAL").ToString();
    _logger.LogInformation("Easy Auth Principal Header: {PrincipalHeader}", principalHeader);
    if (!string.IsNullOrEmpty(principalHeader))
    {
      var decoded = Convert.FromBase64String(principalHeader);
      var json = Encoding.UTF8.GetString(decoded);
      var principal = JsonDocument.Parse(json);

      var claims = new List<Claim>();
      foreach (var claim in principal.RootElement.GetProperty("claims").EnumerateArray())
      {
        var type = claim.GetProperty("typ").GetString() ?? string.Empty;
        var value = claim.GetProperty("val").GetString() ?? string.Empty;
        claims.Add(new Claim(type, value));
      }

      var identity = new ClaimsIdentity(claims, "EasyAuth");
      context.Items["User"] = new ClaimsPrincipal(identity);
    }

    // Call the next middleware in the pipeline
    await _next(context);
  }
}