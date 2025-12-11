using System.Net.Http.Json;
using RGS.Backend.Shared.Models;
using RGS.Backend.Shared.ViewModels;
using RGS.Frontend;

internal interface IResumeDataService
{
  Task<ResumeData> GetMasterResumeDataAsync();
  Task SetMasterResumeDataAsync(ResumeData resumeData);
}

internal class ResumeDataService(HttpClient httpClient, ILogger<PostingsService> logger) : IResumeDataService
{
  private readonly HttpClient _httpClient = httpClient;
  private readonly ILogger<PostingsService> _logger = logger;

  public async Task<ResumeData> GetMasterResumeDataAsync()
  {
    return await _httpClient.GetFromJsonAsync<ResumeData>($"/api/GetResumeData?postingId=master") ?? throw new RGSException("Failed to retrieve master resume data");
  }

  public async Task SetMasterResumeDataAsync(ResumeData resumeData)
  {
    await _httpClient.PostAsync("/api/SetResumeData", JsonContent.Create(resumeData));
  }
}
