using System.Net.Http.Json;
using RGS.Backend.Shared.Models;
using RGS.Backend.Shared.ViewModels;
using RGS.Frontend;

internal interface IResumeDataService
{
  Task<ResumeDataModel> GetMasterResumeDataAsync();
  Task SetMasterResumeDataAsync(ResumeDataModel resumeData);
}

internal class ResumeDataService(HttpClient httpClient, ILogger<PostingsService> logger) : IResumeDataService
{
  private readonly HttpClient _httpClient = httpClient;
  private readonly ILogger<PostingsService> _logger = logger;

  public async Task<ResumeDataModel> GetMasterResumeDataAsync()
  {
    return await _httpClient.GetFromJsonAsync<ResumeDataModel>($"/api/GetResumeData?postingId=master") ?? throw new RGSException("Failed to retrieve master resume data");
  }

  public async Task SetMasterResumeDataAsync(ResumeDataModel resumeData)
  {
    await _httpClient.PostAsync("/api/SetResumeData", JsonContent.Create(resumeData));
  }
}
