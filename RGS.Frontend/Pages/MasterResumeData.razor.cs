using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using R3;
using RGS.Backend.Shared.ViewModels;

namespace RGS.Frontend.Pages;

public partial class MasterResumeData : ComponentBase, IDisposable
{
  private EditForm? editForm;
  private ResumeDataModel resumeData = null!;
  private bool _disposedValue;
  private IDisposable? _subscription;

  [Inject] private IResumeDataService ResumeDataService { get; set; } = null!;
  [Inject] private ILogger<MasterResumeData> Logger { get; set; } = null!;

  protected override async Task OnInitializedAsync()
  {
    // Load master resume data
    resumeData = await ResumeDataService.GetMasterResumeDataAsync();
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    if (_subscription is null && editForm is not null)
    {
      Logger.LogInformation("EditForm is null? {isNull}", editForm is null);
      Logger.LogInformation("EditContext is null? {isNull}", editForm?.EditContext is null);
      _subscription = Observable.FromEventHandler<FieldChangedEventArgs>(
        a => editForm!.EditContext!.OnFieldChanged += a,
        a => editForm!.EditContext!.OnFieldChanged -= a)
        .Debounce(TimeSpan.FromMilliseconds(500))
        .Where(_ => editForm!.EditContext!.Validate())
        .Do(async () =>
        {
          Logger.LogInformation("Handling submit from subscription");
          editForm!.EditContext!.MarkAsUnmodified();
          await HandleValidSubmit(editForm.EditContext);
        })
        .Subscribe();
    }
  }

  private async Task HandleValidSubmit(EditContext args)
  {
    ResumeDataModel data = (ResumeDataModel)args.Model;
    await ResumeDataService.SetMasterResumeDataAsync(data);
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