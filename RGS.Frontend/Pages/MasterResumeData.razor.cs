using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Blazilla;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using R3;
using RGS.Backend.Shared.Models;
using RGS.Backend.Shared.ViewModels;
using RGS.Frontend.Components;

namespace RGS.Frontend.Pages;

public partial class MasterResumeData : ComponentBase, IDisposable
{
  private EditContext? editContext;
  private ResumeDataModel resumeData = null!;
  private bool _disposedValue;
  private IDisposable? _subscription;
  private IDisposable? _otherSubscription;

  [Inject] private IResumeDataService ResumeDataService { get; set; } = null!;
  [Inject] private ILogger<MasterResumeData> Logger { get; set; } = null!;
  [Inject] private IServiceProvider _serviceProvider { get; set; } = null!;

  private string NewCategory { get; set; } = "";

  protected override async Task OnInitializedAsync()
  {
    // Load master resume data
    resumeData = (await ResumeDataService.GetMasterResumeDataAsync()).Wrap();
    editContext = new(resumeData);
    _otherSubscription?.Dispose();
    _otherSubscription = editContext.EnableDataAnnotationsValidation(_serviceProvider);
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (_subscription is null && editContext is not null)
    {
      _subscription = Observable.FromEventHandler<FieldChangedEventArgs>(
        a => editContext!.OnFieldChanged += a,
        a => editContext!.OnFieldChanged -= a)
        .Select(sa => sa.sender as EditContext)
        .Where(ec => ec?.Validate() ?? false)
        .Debounce(TimeSpan.FromMilliseconds(500))
        .SubscribeAwait(async (ec, ct) =>
        {
          await HandleSubmit(ec!);
          ec!.MarkAsUnmodified();
          StateHasChanged();
        }, AwaitOperation.ThrottleFirstLast);
    }
  }

  private async Task OnSkillCategoryChanged(SkillCategory changed, int index)
  {
    resumeData.Skills = [.. resumeData.Skills.Select((skill, i) => i == index ? changed : skill)];
    editContext!.NotifyFieldChanged(FieldIdentifier.Create(() => resumeData.Skills));
  }

  private async Task OnSkillCategoryAdded(string category)
  {
    NewCategory = "";
    resumeData.Skills.Add(new SkillCategory(category, []));
    editContext!.NotifyFieldChanged(FieldIdentifier.Create(() => resumeData.Skills));
  }

  private async Task OnSkillCategoryRemoved(int index)
  {
    resumeData.Skills.RemoveAt(index);
    editContext!.NotifyFieldChanged(FieldIdentifier.Create(() => resumeData.Skills));
  }

  private async Task HandleValidSubmit(EditContext args)
  {
    ResumeData data = (ResumeData)args.Model;
    await ResumeDataService.SetMasterResumeDataAsync(data);
  }

  private async Task HandleSubmit(EditContext args)
  {
    ResumeDataModel data = (ResumeDataModel)args.Model;
    if (args.Validate())
    {
      await ResumeDataService.SetMasterResumeDataAsync(data.Unwrap());
    }
    else
    {
      Logger.LogInformation("Invalid submit");
    }
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposedValue)
    {
      if (disposing)
      {
        _subscription?.Dispose();
      }

      _disposedValue = true;
    }
  }

  public void Dispose()
  {
    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}