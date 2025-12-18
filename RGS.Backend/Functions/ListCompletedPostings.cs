using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using RGS.Backend.Shared.Models;
using RGS.Backend.Shared;
using RGS.Backend.Services;
using System.Net;

namespace RGS.Backend.Functions;

internal class ListCompletedPostings(ILogger<ListCompletedPostings> logger, IUserDataRepository userDataRepository)
{
    private readonly ILogger<ListCompletedPostings> _logger = logger;
    private readonly IUserDataRepository _userDataRepository = userDataRepository;

    [Function("ListCompletedPostings")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        DateTime? lastImportedAt = null;
        string? lastId = null;
        string? status = null;
        string? searchText = null;

        if (req.Query.TryGetValue("lastImportedAt", out var lastImportedAtValues))
        {
            if (DateTime.TryParse(lastImportedAtValues.First(), out var parsedDateTime))
            {
                lastImportedAt = parsedDateTime.ToUniversalTime();
                _logger.LogInformation(lastImportedAt.ToString());
            }
            else
            {
                return new BadRequestObjectResult("Invalid lastImportedAt format");
            }
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

        var result = await _userDataRepository.GetPostingListAsync(lastImportedAt, lastId, status, searchText);

        return result.ToJsonActionResult();
    }
}