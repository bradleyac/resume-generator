using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using R3;
using RGS.Backend.Shared.Models;
using RGS.Backend.Shared.ViewModels;

namespace RGS.Frontend.Pages;

public partial class MasterResumeData : ComponentBase, IDisposable
{
  private EditContext? editContext;
  private ResumeDataModel resumeData = null!;
  private bool _disposedValue;
  private IDisposable? _subscription;

  [Inject] private IResumeDataService ResumeDataService { get; set; } = null!;
  [Inject] private ILogger<MasterResumeData> Logger { get; set; } = null!;

  protected override async Task OnInitializedAsync()
  {
    // Load master resume data
    resumeData = await ResumeDataService.GetMasterResumeDataAsync();
    editContext = new(resumeData);
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
        .Do(async (ec) =>
        {
          ec!.MarkAsUnmodified();
          await HandleValidSubmit(ec);
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