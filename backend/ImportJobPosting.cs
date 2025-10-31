using System.Collections.Specialized;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RGS.Backend.Shared.Models;

namespace RGS.Backend;

public class ImportJobPosting(ILogger<ImportJobPosting> logger, CosmosClient cosmosClient, IConfiguration config)
{
    private readonly ILogger<ImportJobPosting> _logger = logger;
    private readonly CosmosClient _cosmosClient = cosmosClient;
    private readonly IConfiguration _config = config;

    [Function("ImportJobPosting")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        try
        {
            var key = _config["Api:Key"];
            var providedKey = req.Headers["x-api-key"].ToString();

            if (string.IsNullOrEmpty(key) || key != providedKey)
            {
                return new UnauthorizedResult();
            }
            // TODO: CSRF protection
            // Currently using cookie-based authentication built into Azure Functions, and thus vulnerable to CSRF.
            // Fixes include changing to token-based authentication in headers or implementing anti-CSRF tokens.
            var postings = _cosmosClient.GetContainer("Resumes", "Postings");
            var payload = await req.ReadFromJsonAsync<NewPostingModel>() ?? throw new ArgumentException("Invalid payload");
            var newPosting = new JobPosting
            (
                Guid.NewGuid().ToString(),
                payload.Link,
                payload.Company,
                payload.Title,
                payload.PostingText,
                DateTime.UtcNow
            );
            await postings.UpsertItemAsync(newPosting);
            return new OkResult();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to import job posting");
            throw;
        }
    }
}