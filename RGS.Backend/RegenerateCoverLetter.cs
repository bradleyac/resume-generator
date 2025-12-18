using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RGS.Backend.Services;
using RGS.Backend.Shared.Models;

namespace RGS.Backend;

internal class RegenerateCoverLetter(ILogger<RegenerateCoverLetter> logger, PostingProcessor postingProcessor)
{
    private readonly ILogger<RegenerateCoverLetter> _logger = logger;
    private readonly PostingProcessor _postingProcessor = postingProcessor;

    [Function("RegenerateCoverLetter")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        var payload = await req.ReadFromJsonAsync<RegenerateCoverLetterModel>() ?? throw new ArgumentException("Invalid payload");

        _logger.LogInformation("Re-generating cover letter.");
        var result = await _postingProcessor.RegenerateCoverLetterAsync(payload);

        return result switch
        {
            { IsSuccess: true } => new OkResult(),
            { IsSuccess: false, StatusCode: System.Net.HttpStatusCode statusCode } => new StatusCodeResult((int)statusCode),
            _ => new StatusCodeResult((int)System.Net.HttpStatusCode.InternalServerError),
        };
    }
}