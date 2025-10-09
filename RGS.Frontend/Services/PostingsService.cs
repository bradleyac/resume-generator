using System.Net.Http.Json;
using RGS.Backend.Shared.Models;
using RGS.Frontend;

internal interface IPostingsService
{
  Task<CompletedPosting> GetPostingAsync(string postingId);
  Task<List<PostingSummary>> GetPostingsAsync();
  IAsyncEnumerable<List<PostingSummary>> GetPostingsStreamAsync(string? status = null);
  Task<ResumeData> GetResumeDataAsync(string postingId);
  Task SubmitNewPostingAsync(NewPostingModel model);
  Task SetPostingStatusAsync(PostingStatusUpdate statusUpdate);
}

internal class PostingsService(HttpClient httpClient, ILogger<PostingsService> logger) : IPostingsService
{
  private readonly HttpClient _httpClient = httpClient;
  private readonly ILogger<PostingsService> _logger = logger;

  public async Task<CompletedPosting> GetPostingAsync(string postingId)
  {
    return await _httpClient.GetFromJsonAsync<CompletedPosting>($"/api/GetCompletedPosting?completedPostingId={postingId}") ?? throw new RGSException("Failed to retrieve posting");
  }

  public async Task<List<PostingSummary>> GetPostingsAsync()
  {
    return await _httpClient.GetFromJsonAsync<List<PostingSummary>>($"/api/ListCompletedPostings") ?? throw new RGSException("Failed to retrieve postings");
  }

  public async Task<ResumeData> GetResumeDataAsync(string postingId)
  {
    return await _httpClient.GetFromJsonAsync<ResumeData>($"/api/GetResumeData?postingId={postingId}") ?? throw new RGSException("Failed to retrieve resume data");
  }

  public async Task SubmitNewPostingAsync(NewPostingModel model)
  {
    await _httpClient.PostAsync("/api/ImportJobPosting", JsonContent.Create(model));
  }

  public async Task SetPostingStatusAsync(PostingStatusUpdate statusUpdate)
  {
    await _httpClient.PostAsync("/api/SetPostingStatus", JsonContent.Create(statusUpdate));
  }

  public async IAsyncEnumerable<List<PostingSummary>> GetPostingsStreamAsync(string? status = null)
  {
    DateTime? lastImportedAt = null;
    string? lastId = null;

    // TODO: Not really using IAsyncEnumerable correctly here
    // The API may stream the result but we batch it here so we don't benefit.
    while (await fetchNextBatch() is var batch && (batch?.Any() ?? false))
    {
      PostingSummary lastElement = batch!.Last();
      lastImportedAt = lastElement.ImportedAt;
      lastId = lastElement.id;
      yield return batch;
    }

    Task<List<PostingSummary>?> fetchNextBatch()
    {
      _logger.LogInformation(lastImportedAt.ToString());
      Dictionary<string, string?> queryParamList = new()
      {
        { "status", status },
        { "lastImportedAt", lastImportedAt?.ToString("o") },
        { "lastId", lastId?.ToString() }
      };
      string queryParams = string.Join("&", queryParamList.Where(param => param.Value != null).Select(param => $"{param.Key}={param.Value}"));
      var url = $"/api/ListCompletedPostings?{queryParams}";
      Console.WriteLine(url);
      return _httpClient.GetFromJsonAsync<List<PostingSummary>>(url);
    }
  }
}
