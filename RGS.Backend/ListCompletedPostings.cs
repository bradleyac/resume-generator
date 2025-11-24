using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using RGS.Backend.Shared.Models;
using RGS.Backend.Shared;

namespace RGS.Backend;

public class ListCompletedPostings(ILogger<ListCompletedPostings> logger, CosmosClient cosmosClient)
{
    private readonly ILogger<ListCompletedPostings> _logger = logger;
    private readonly CosmosClient _cosmosClient = cosmosClient;

    private const int MaxPostingsToReturn = 10;

    [Function("ListCompletedPostings")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        DateTime? lastImportedAt = null;
        string? lastId = null;
        string? status = null;
        string? searchText = null;

        if (req.Query.TryGetValue("lastImportedAt", out var lastImportedAtValues))
        {
            lastImportedAt = DateTime.Parse(lastImportedAtValues.First()).ToUniversalTime();
            _logger.LogInformation(lastImportedAt.ToString());
        }

        if (req.Query.TryGetValue("lastId", out var lastIdValues))
        {
            lastId = lastIdValues.First();
        }

        if (req.Query.TryGetValue("status", out var statusValues))
        {
            status = statusValues.First();
        }

        if (req.Query.TryGetValue("searchText", out var searchTextValues))
        {
            searchText = searchTextValues.First();
        }

        var completedPostingsContainer = _cosmosClient.GetContainer("Resumes", "Postings");
        var query = completedPostingsContainer.GetItemLinqQueryable<JobPosting>()
            .Where(p => lastImportedAt == null || (p.ImportedAt == lastImportedAt && p.id.CompareTo(lastId) > 0) || p.ImportedAt < lastImportedAt)
            .Where(p => status == null || p.Status == status)
            .Where(p => string.IsNullOrWhiteSpace(searchText) || p.Company.FullTextContains(searchText) || p.Title.FullTextContains(searchText) || p.PostingText.FullTextContains(searchText))
            .Select(p => new { p.id, p.Company, p.Title, p.Link, p.ImportedAt, p.Status })
            .OrderByDescending(p => p.ImportedAt)
            .ThenBy(p => p.id)
            .Take(MaxPostingsToReturn);

        var list = await query.ToFeedIterator().ToListAsync();
        return new JsonResult(list.Select(item => new PostingSummary(item.id, item.Link, item.Company, item.Title, item.ImportedAt, item.Status)).ToList());
    }
}