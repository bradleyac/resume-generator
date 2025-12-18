using System.Net.Http.Json;
using RGS.Backend.Shared.Models;
using RGS.Backend.Shared.ViewModels;
using RGS.Frontend;

internal interface IResumeDataService
{
  Task<SourceResumeData> GetSourceResumeDataAsync();
  Task SetSourceResumeDataAsync(SourceResumeData resumeData);
}

internal class ResumeDataService(HttpClient httpClient, ILogger<PostingsService> logger) : IResumeDataService
{
  private readonly HttpClient _httpClient = httpClient;
  private readonly ILogger<PostingsService> _logger = logger;

  public async Task<SourceResumeData> GetSourceResumeDataAsync()
  {
    return await _httpClient.GetFromJsonAsync<SourceResumeData>($"/api/GetSourceResumeData") ?? throw new RGSException("Failed to retrieve source resume data");
  }

  public async Task SetSourceResumeDataAsync(SourceResumeData resumeData)
  {
    await _httpClient.PostAsync("/api/SetSourceResumeData", JsonContent.Create(resumeData));
  }
}
