using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RGS.Backend.Services;
using RGS.Backend.Shared.Models;

namespace RGS.Backend;

internal class ImportJobPosting(ILogger<ImportJobPosting> logger, ICurrentUserService currentUserService, IUserService userService, IUserDataRepositoryFactory userDataRepositoryFactory)
{
    private readonly ILogger<ImportJobPosting> _logger = logger;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IUserService _userService = userService;
    private readonly IUserDataRepositoryFactory _userDataRepositoryFactory = userDataRepositoryFactory;

    // Requires either user authentication or an API key in the "x-api-key" header
    [Function("ImportJobPosting")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        try
        {
            // TODO: This is more vulnerable to CSRF for allowing cookie-based auth here as well.

            // See if user is authenticated, first
            var currentUserId = _currentUserService.GetCurrentUserId();

            // If not, check for API key
            if (currentUserId is null)
            {
                var providedKey = req.Headers["x-api-key"].ToString().Trim();

                if (string.IsNullOrEmpty(providedKey))
                {
                    return new UnauthorizedResult();
                }

                var user = await _userService.GetUserByApiKeyAsync(providedKey);

                if (user is null || user.ApiKey != providedKey)
                {
                    return new UnauthorizedResult();
                }

                currentUserId = user.id;
            }

            var userDataRepository = _userDataRepositoryFactory.CreateUserDataRepository(currentUserId);

            var payload = await req.ReadFromJsonAsync<NewPostingModel>() ?? throw new ArgumentException("Invalid payload");

            var newPosting = new JobPosting
            (
                Guid.NewGuid().ToString(),
                currentUserId,
                DateTime.UtcNow,
                new PostingDetails(payload.Link, payload.Company, payload.Title, payload.PostingText)
            );

            return await userDataRepository.SetPostingAsync(newPosting) switch
            {
                true => new OkResult(),
                false => new StatusCodeResult((int)HttpStatusCode.InternalServerError),
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to import job posting");
            throw;
        }
    }
}