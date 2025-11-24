using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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

    var principalHeader = req.Headers.Single(kvp => kvp.Key == "X-MS-CLIENT-PRINCIPAL").Value.Single();
    _logger.LogInformation("Easy Auth Principal Header: {PrincipalHeader}", principalHeader);
    if (!string.IsNullOrEmpty(principalHeader))
    {
      var decoded = Convert.FromBase64String(principalHeader);
      var json = Encoding.UTF8.GetString(decoded);
      var principal = JsonSerializer.Deserialize<EasyAuthUser>(json);
      _logger.LogInformation("Easy Auth Principal JSON: {PrincipalJson}", json);

      context.Items["User"] = principal;
    }

    // Call the next middleware in the pipeline
    await _next(context);
  }
}

internal record EasyAuthUser(string IdentityProvider, string UserId, string UserDetails, string[] UserRoles);