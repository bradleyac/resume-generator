using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace RGS.Functions;

public class ImportJobPosting
{
    private readonly ILogger<ImportJobPosting> _logger;

    public ImportJobPosting(ILogger<ImportJobPosting> logger)
    {
        _logger = logger;
    }

    [Function("ImportJobPosting")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        try
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            Microsoft.Azure.Cosmos.CosmosClient client = new(accountEndpoint: "https://resume-generation-system.documents.azure.com:443/", tokenCredential: new DefaultAzureCredential());
            var pendingPostings = client.GetContainer("Resumes", "PendingPostings");
            var payload = await req.ReadFromJsonAsync<JobPostingPayload>();
            await pendingPostings.UpsertItemAsync(new { Id = Guid.NewGuid().ToString(), payload.PostingText });
            return new OkResult();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to import job posting");
            throw;
        }
    }
}

public record JobPostingPayload(string PostingText);