using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace RGS.Backend.Middleware;

internal class EasyAuthMiddleware : IFunctionsWorkerMiddleware
{
  private readonly ILogger<EasyAuthMiddleware> _logger;

  public EasyAuthMiddleware(ILogger<EasyAuthMiddleware> logger)
  {
    _logger = logger;
  }

  public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
  {
    if (context is null)
    {
      await next(context);
      return;
    }
    // Extract user information from Easy Auth headers
    var req = await context.GetHttpRequestDataAsync();

    var principalHeader = req.Headers.Single(kvp => kvp.Key == "X-MS-CLIENT-PRINCIPAL").Value.Single();
    if (!string.IsNullOrEmpty(principalHeader))
    {
      var principal = JsonSerializer.Deserialize<EasyAuthUser>(Convert.FromBase64String(principalHeader));
      context.Items["User"] = principal;
    }

    await next(context);
  }
}

internal class EasyAuthUser
{
  [JsonPropertyName("userId")]
  public string UserId { get; set; } = string.Empty;

  [JsonPropertyName("userDetails")]
  public string UserDetails { get; set; } = string.Empty;

  [JsonPropertyName("identityProvider")]
  public string IdentityProvider { get; set; } = string.Empty;

  [JsonPropertyName("userRoles")]
  public string[] UserRoles { get; set; } = [];
}