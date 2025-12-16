using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker.Extensions.Abstractions;
using RGS.Backend.Services;
using RGS.Backend.Shared;
using RGS.Backend.Shared.Models;

internal interface IUserDataRepositoryFactory
{
  IUserDataRepository CreateUserDataRepository(string userId);
}

internal class UserDataRepositoryFactory(CosmosClient cosmosClient, IUserService userService) : IUserDataRepositoryFactory
{
  public IUserDataRepository CreateUserDataRepository(string userId)
  {
    return new UserDataRepository(cosmosClient, userService, userId);
  }
}

internal interface IUserDataRepository
{
  Task<ResumeData?> GetResumeDataAsync(string postingId);
  Task<SourceResumeData?> GetSourceResumeDataAsync();
  Task<SourceResumeData?> GetSourceResumeDataAsync(string resumeDataId);
  Task<bool> SetResumeDataAsync(ResumeData resumeData);
  Task<bool> SetSourceResumeDataAsync(SourceResumeData sourceResumeData);
  Task<JobPosting?> GetPostingAsync(string postingId);
  Task<bool> SetPostingAsync(JobPosting posting);
  Task<bool> DeletePostingAsync(string postingId);
  Task<bool> SetPostingStatusAsync(string postingId, string status);
  Task<bool> SetPostingAddressAsync(UpdatePostingAddressModel update);
  Task<bool> SetCoverLetterAsync(string postingId, CoverLetter coverLetter);
  Task<List<PostingSummary>> GetPostingListAsync(DateTime? lastImportedAt, string? lastId, string? status, string? searchText);
}

internal partial class UserDataRepository : IUserDataRepository
{
  private string UserId { get; set; }
  private CosmosClient CosmosClient { get; set; }
  private IUserService UserService { get; set; }

  // Get current user ID from userService
  public UserDataRepository(CosmosClient cosmosClient, IUserService userService)
  {
    // TODO: Not great to throw here. What's a better overall pattern for handling errors like this in ASP.NET Core?
    // There are too many different ways for an error to surface, it should be consistent.
    UserId = userService.GetCurrentUserId() ?? throw new InvalidOperationException("No authenticated user present");
    UserService = userService;
    CosmosClient = cosmosClient;
  }

  // Provided user ID
  public UserDataRepository(CosmosClient cosmosClient, IUserService userService, string userId)
  {
    UserId = userId;
    UserService = userService;
    CosmosClient = cosmosClient;
  }

  public async Task<SourceResumeData?> GetSourceResumeDataAsync()
  {
    var container = CosmosClient.GetContainer("Resumes", "UserData");
    var user = await UserService.GetUserByIdAsync(UserId) ?? throw new InvalidOperationException($"User {UserId} not found");
    var response = await container.ReadItemAsync<SourceResumeData>(user.SourceResumeDataId, new PartitionKey(UserId));

    return response.StatusCode switch
    {
      HttpStatusCode.OK => response.Resource,
      _ => null,
    };
  }

  public async Task<SourceResumeData?> GetSourceResumeDataAsync(string resumeDataId)
  {
    var container = CosmosClient.GetContainer("Resumes", "UserData");
    var response = await container.ReadItemAsync<SourceResumeData>(resumeDataId, new PartitionKey(UserId));

    return response.StatusCode switch
    {
      HttpStatusCode.OK => response.Resource,
      _ => null,
    };
  }

  public async Task<bool> SetSourceResumeDataAsync(SourceResumeData sourceResumeData)
  {
    var container = CosmosClient.GetContainer("Resumes", "UserData");
    var response = await container.UpsertItemAsync(sourceResumeData);

    return response.StatusCode == HttpStatusCode.OK;
  }
  public async Task<JobPosting?> GetPostingAsync(string postingId)
  {
    var container = CosmosClient.GetContainer("Resumes", "UserData");
    var response = await container.ReadItemAsync<JobPosting>(postingId, new PartitionKey(UserId));

    return response.StatusCode switch
    {
      HttpStatusCode.OK => response.Resource,
      _ => null,
    };
  }

  public async Task<bool> SetPostingAsync(JobPosting posting)
  {
    var container = CosmosClient.GetContainer("Resumes", "UserData");
    var response = await container.UpsertItemAsync(posting);

    return response.StatusCode == HttpStatusCode.OK;
  }

  public async Task<bool> SetPostingStatusAsync(string postingId, string status)
  {
    var container = CosmosClient.GetContainer("Resumes", "UserData");
    var response = await container.PatchItemAsync<JobPosting>(postingId, new PartitionKey(UserId), [PatchOperation.Set("Status", status)]);

    return response.StatusCode == HttpStatusCode.OK;
  }

  public async Task<bool> SetCoverLetterAsync(string postingId, CoverLetter coverLetter)
  {
    var container = CosmosClient.GetContainer("Resumes", "UserData");
    var response = await container.PatchItemAsync<JobPosting>(postingId, new PartitionKey(UserId), [PatchOperation.Set("CoverLetter", coverLetter)]);

    return response.StatusCode == HttpStatusCode.OK;
  }

  public async Task<ResumeData?> GetResumeDataAsync(string postingId)
  {
    var container = CosmosClient.GetContainer("Resumes", "UserData");
    var response = await container.ReadItemAsync<JobPosting>(postingId, new PartitionKey(UserId));

    return response.StatusCode switch
    {
      HttpStatusCode.OK => response.Resource.ResumeData,
      _ => null,
    };
  }

  public async Task<bool> DeletePostingAsync(string postingId)
  {
    var container = CosmosClient.GetContainer("Resumes", "UserData");
    var response = await container.DeleteItemAsync<JobPosting>(postingId, new PartitionKey(UserId));

    return response.StatusCode == HttpStatusCode.OK;
  }

  public async Task<List<PostingSummary>> GetPostingListAsync(DateTime? lastImportedAt, string? lastId, string? status, string? searchText)
  {
    const int MaxPostingsToReturn = 10;

    var container = CosmosClient.GetContainer("Resumes", "UserData");
    var query = container.GetItemLinqQueryable<JobPosting>()
        .Where(p => p.UserId == UserId)
        .Where(p => lastImportedAt == null || (p.ImportedAt == lastImportedAt && p.id.CompareTo(lastId) > 0) || p.ImportedAt < lastImportedAt)
        .Where(p => status == null || p.Status == status)
        .Where(p => string.IsNullOrWhiteSpace(searchText) || p.PostingData.Company.FullTextContains(searchText) || p.PostingData.Title.FullTextContains(searchText) || p.PostingData.PostingText.FullTextContains(searchText))
        .Select(p => new PostingSummary(p.id, p.PostingData.Company, p.PostingData.Title, p.PostingData.Link, p.ImportedAt, p.Status))
        .OrderByDescending(p => p.ImportedAt)
        .ThenBy(p => p.id)
        .Take(MaxPostingsToReturn);

    return await query.ToFeedIterator().ToListAsync();
  }

  public async Task<bool> SetResumeDataAsync(ResumeData resumeData)
  {
    var container = CosmosClient.GetContainer("Resumes", "UserData");
    var response = await container.UpsertItemAsync(resumeData);

    return response.StatusCode == HttpStatusCode.OK;
  }

  public async Task<bool> SetPostingAddressAsync(UpdatePostingAddressModel update)
  {
    var container = CosmosClient.GetContainer("Resumes", "UserData");
    var response = await container.PatchItemAsync<JobPosting>(update.PostingId, new PartitionKey(UserId),
    [
      PatchOperation.Set("PostingData.StreetAddress", update.StreetAddress),
      PatchOperation.Set("PostingData.City", update.City),
      PatchOperation.Set("PostingData.State", update.State),
      PatchOperation.Set("PostingData.Zip", update.Zip)
    ]);

    return response.StatusCode == HttpStatusCode.OK;
  }
}