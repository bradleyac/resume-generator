using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using RGS.Backend.Services;
using RGS.Backend.Shared;
using RGS.Backend.Shared.Models;

namespace RGS.Backend.Services;

internal interface IUserDataRepositoryFactory
{
  IUserDataRepository CreateUserDataRepository(string userId);
}

internal class UserDataRepositoryFactory(CosmosClient cosmosClient, IUserService userService, ILogger<UserDataRepository> logger) : IUserDataRepositoryFactory
{
  public IUserDataRepository CreateUserDataRepository(string userId)
  {
    return new UserDataRepository(cosmosClient, userService, userId, logger);
  }
}

internal interface IUserDataRepository
{
  Task<Result<ResumeData>> GetResumeDataAsync(string postingId);
  Task<Result<SourceResumeData>> GetSourceResumeDataAsync();
  Task<Result<SourceResumeData>> GetSourceResumeDataAsync(string resumeDataId);
  Task<Result> SetResumeDataAsync(ResumeData resumeData);
  Task<Result> SetSourceResumeDataAsync(SourceResumeData sourceResumeData);
  Task<Result<JobPosting>> GetPostingAsync(string postingId);
  Task<Result> SetPostingAsync(JobPosting posting);
  Task<Result> DeletePostingAsync(string postingId);
  Task<Result> SetPostingStatusAsync(string postingId, string status);
  Task<Result> SetPostingAddressAsync(UpdatePostingAddressModel update);
  Task<Result> SetCoverLetterAsync(string postingId, CoverLetter coverLetter);
  Task<Result<List<PostingSummary>>> GetPostingListAsync(DateTime? lastImportedAt, string? lastId, string? status, string? searchText);
}

internal partial class UserDataRepository : IUserDataRepository
{
  private string UserId { get; set; }
  private CosmosClient CosmosClient { get; set; }
  private IUserService UserService { get; set; }
  private ILogger<UserDataRepository> Logger { get; init; }

  // Get current user ID from userService
  public UserDataRepository(CosmosClient cosmosClient, ICurrentUserService currentUserService, IUserService userService, ILogger<UserDataRepository> logger)
  {
    // TODO: Not great to throw here. What's a better overall pattern for handling errors like this in ASP.NET Core?
    // There are too many different ways for an error to surface, it should be consistent.
    UserId = currentUserService.GetCurrentUserId() ?? throw new InvalidOperationException("No authenticated user present");
    UserService = userService;
    CosmosClient = cosmosClient;
    Logger = logger;
  }

  // Provided user ID
  public UserDataRepository(CosmosClient cosmosClient, IUserService userService, string userId, ILogger<UserDataRepository> logger)
  {
    UserId = userId;
    UserService = userService;
    CosmosClient = cosmosClient;
    Logger = logger;
  }

  public async Task<Result<SourceResumeData>> GetSourceResumeDataAsync()
  {
    try
    {
      var container = CosmosClient.GetContainer("Resumes", "UserData");
      var user = await UserService.GetUserByIdAsync(UserId) ?? throw new InvalidOperationException($"User {UserId} not found");
      var response = await container.ReadItemAsync<SourceResumeData>(user.SourceResumeDataId, new PartitionKey(UserId));

      return response.StatusCode switch
      {
        HttpStatusCode.OK => Result<SourceResumeData>.Success(response.Resource),
        _ => Result<SourceResumeData>.Failure("Failed to retrieve source resume data", response.StatusCode.FromCosmosDBStatusCode()),
      };
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Exception occurred while retrieving source resume data");
      return Result<SourceResumeData>.Failure($"Exception occurred while retrieving source resume data: {ex.Message}", HttpStatusCode.InternalServerError);
    }
  }

  public async Task<Result<SourceResumeData>> GetSourceResumeDataAsync(string resumeDataId)
  {
    try
    {
      var container = CosmosClient.GetContainer("Resumes", "UserData");
      var response = await container.ReadItemAsync<SourceResumeData>(resumeDataId, new PartitionKey(UserId));

      return response.StatusCode switch
      {
        HttpStatusCode.OK => Result<SourceResumeData>.Success(response.Resource),
        _ => Result<SourceResumeData>.Failure("Failed to retrieve source resume data", response.StatusCode.FromCosmosDBStatusCode()),
      };
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Exception occurred while retrieving source resume data");
      return Result<SourceResumeData>.Failure($"Exception occurred while retrieving source resume data: {ex.Message}", HttpStatusCode.InternalServerError);
    }
  }

  public async Task<Result> SetSourceResumeDataAsync(SourceResumeData sourceResumeData)
  {
    try
    {
      var container = CosmosClient.GetContainer("Resumes", "UserData");
      var response = await container.UpsertItemAsync(sourceResumeData);

      return response.StatusCode == HttpStatusCode.OK ? Result.Success() : Result.Failure("Failed to set source resume data", response.StatusCode.FromCosmosDBStatusCode());
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Exception occurred while setting source resume data");
      return Result.Failure($"Exception occurred while setting source resume data: {ex.Message}", HttpStatusCode.InternalServerError);
    }
  }
  public async Task<Result<JobPosting>> GetPostingAsync(string postingId)
  {
    try
    {
      var container = CosmosClient.GetContainer("Resumes", "UserData");
      var response = await container.ReadItemAsync<JobPosting>(postingId, new PartitionKey(UserId));

      return response.StatusCode switch
      {
        HttpStatusCode.OK => Result<JobPosting>.Success(response.Resource),
        _ => Result<JobPosting>.Failure("Failed to retrieve job posting", response.StatusCode.FromCosmosDBStatusCode()),
      };
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Exception occurred while retrieving job posting");
      return Result<JobPosting>.Failure($"Exception occurred while retrieving job posting: {ex.Message}", HttpStatusCode.InternalServerError);
    }
  }

  public async Task<Result> SetPostingAsync(JobPosting posting)
  {
    try
    {

      var container = CosmosClient.GetContainer("Resumes", "UserData");
      var response = await container.UpsertItemAsync(posting);

      return response.StatusCode == HttpStatusCode.OK ? Result.Success() : Result.Failure("Failed to set job posting", response.StatusCode.FromCosmosDBStatusCode());
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Exception occurred while setting job posting");
      return Result.Failure($"Exception occurred while setting job posting: {ex.Message}", HttpStatusCode.InternalServerError);
    }
  }

  public async Task<Result> SetPostingStatusAsync(string postingId, string status)
  {
    try
    {
      var container = CosmosClient.GetContainer("Resumes", "UserData");
      var response = await container.PatchItemAsync<JobPosting>(postingId, new PartitionKey(UserId), [PatchOperation.Set("Status", status)]);

      return response.StatusCode == HttpStatusCode.OK ? Result.Success() : Result.Failure("Failed to set job posting status", response.StatusCode.FromCosmosDBStatusCode());
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Exception occurred while setting job posting status");
      return Result.Failure($"Exception occurred while setting job posting status: {ex.Message}", HttpStatusCode.InternalServerError);
    }
  }

  public async Task<Result> SetCoverLetterAsync(string postingId, CoverLetter coverLetter)
  {
    try
    {
      var container = CosmosClient.GetContainer("Resumes", "UserData");
      var response = await container.PatchItemAsync<JobPosting>(postingId, new PartitionKey(UserId), [PatchOperation.Set("CoverLetter", coverLetter)]);

      return response.StatusCode == HttpStatusCode.OK ? Result.Success() : Result.Failure("Failed to set cover letter", response.StatusCode.FromCosmosDBStatusCode());
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Exception occurred while setting cover letter");
      return Result.Failure($"Exception occurred while setting cover letter: {ex.Message}", HttpStatusCode.InternalServerError);
    }
  }

  public async Task<Result<ResumeData>> GetResumeDataAsync(string postingId)
  {
    try
    {
      var container = CosmosClient.GetContainer("Resumes", "UserData");
      var response = await container.ReadItemAsync<JobPosting>(postingId, new PartitionKey(UserId));

      return response.StatusCode switch
      {
        HttpStatusCode.OK => response?.Resource?.ResumeData switch
        {
          ResumeData resumeData => Result<ResumeData>.Success(resumeData),
          _ => Result<ResumeData>.Failure("Resume data not found on posting", HttpStatusCode.InternalServerError),
        },
        _ => Result<ResumeData>.Failure("Failed to retrieve resume data", response.StatusCode.FromCosmosDBStatusCode()),
      };
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Exception occurred while retrieving resume data");
      return Result<ResumeData>.Failure($"Exception occurred while retrieving resume data: {ex.Message}", HttpStatusCode.InternalServerError);
    }
  }

  public async Task<Result> DeletePostingAsync(string postingId)
  {
    try
    {
      var container = CosmosClient.GetContainer("Resumes", "UserData");
      var response = await container.DeleteItemAsync<JobPosting>(postingId, new PartitionKey(UserId));

      return response.StatusCode == HttpStatusCode.OK ? Result.Success() : Result.Failure("Failed to delete posting", response.StatusCode.FromCosmosDBStatusCode());
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Exception occurred while deleting posting");
      return Result.Failure($"Exception occurred while deleting posting: {ex.Message}", HttpStatusCode.InternalServerError);
    }
  }

  public async Task<Result<List<PostingSummary>>> GetPostingListAsync(DateTime? lastImportedAt, string? lastId, string? status, string? searchText)
  {
    try
    {
      const int MaxPostingsToReturn = 10;

      var container = CosmosClient.GetContainer("Resumes", "UserData");

      // TODO: This is tricky because not all IQueryable methods are supported by Cosmos DB LINQ provider.
      // Need to ensure that any methods used here can be translated to Cosmos DB queries.
      // Also tricky because $type isn't mapped to a property that can be queried against.
      var query = container.GetItemLinqQueryable<JobPosting>(requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(UserId) })
          .Where(p => p.UserId == UserId)
          .Where(p => p.Status != null) // This is doing a lot of work to exclude non-JobPosting types
          .Where(p => lastImportedAt == null || (p.ImportedAt == lastImportedAt && p.id.CompareTo(lastId) > 0) || p.ImportedAt < lastImportedAt)
          .Where(p => status == null || p.Status == status)
          .Where(p => string.IsNullOrWhiteSpace(searchText) || p.PostingData.Company.FullTextContains(searchText) || p.PostingData.Title.FullTextContains(searchText) || p.PostingData.PostingText.FullTextContains(searchText))
          .Select(p => new { p.id, p.PostingData.Company, p.PostingData.Title, p.PostingData.Link, p.ImportedAt, p.Status })
          .OrderByDescending(p => p.ImportedAt)
          .ThenBy(p => p.id)
          .Take(MaxPostingsToReturn);

      return Result<List<PostingSummary>>.Success((await query.ToFeedIterator().ToListAsync()).Select(p => new PostingSummary(p.id, p.Link, p.Company, p.Title, p.ImportedAt, p.Status)).ToList());
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Exception occurred while retrieving posting list");
      return Result<List<PostingSummary>>.Failure($"Exception occurred while retrieving posting list: {ex.Message}", HttpStatusCode.InternalServerError);
    }
  }

  public async Task<Result> SetResumeDataAsync(ResumeData resumeData)
  {
    try
    {
      var container = CosmosClient.GetContainer("Resumes", "UserData");
      var response = await container.UpsertItemAsync(resumeData);

      return response.StatusCode == HttpStatusCode.OK ? Result.Success() : Result.Failure("Failed to set resume data", response.StatusCode.FromCosmosDBStatusCode());
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Exception occurred while setting resume data");
      return Result.Failure($"Exception occurred while setting resume data: {ex.Message}", HttpStatusCode.InternalServerError);
    }
  }

  public async Task<Result> SetPostingAddressAsync(UpdatePostingAddressModel update)
  {
    try
    {
      var container = CosmosClient.GetContainer("Resumes", "UserData");
      var response = await container.PatchItemAsync<JobPosting>(update.PostingId, new PartitionKey(UserId),
      [
        PatchOperation.Set("PostingData.StreetAddress", update.StreetAddress),
        PatchOperation.Set("PostingData.City", update.City),
        PatchOperation.Set("PostingData.State", update.State),
        PatchOperation.Set("PostingData.Zip", update.Zip)
      ]);

      return response.StatusCode == HttpStatusCode.OK ? Result.Success() : Result.Failure("Failed to set posting address", response.StatusCode.FromCosmosDBStatusCode());
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Exception occurred while setting posting address");
      return Result.Failure($"Exception occurred while setting posting address: {ex.Message}", HttpStatusCode.InternalServerError);
    }
  }
}