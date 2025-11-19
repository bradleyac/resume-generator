using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using RGS.Backend.Services;
using RGS.Backend.Shared.Models;

namespace RGS.Backend;

public class RegenerateCoverLetter(ILogger<RegenerateCoverLetter> logger, PostingProcessor postingProcessor)
{
    private readonly ILogger<RegenerateCoverLetter> _logger = logger;
    private readonly PostingProcessor _postingProcessor = postingProcessor;

    [Function("RegenerateCoverLetter")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogInformation("Re-generating cover letter.");
        var payload = await req.ReadFromJsonAsync<RegenerateCoverLetterModel>() ?? throw new ArgumentException("Invalid payload");
        await _postingProcessor.RegenerateCoverLetterAsync(payload);
        return new OkResult();
    }
}