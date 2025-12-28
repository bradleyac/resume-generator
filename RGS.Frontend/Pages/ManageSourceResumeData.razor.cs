using System.ComponentModel;
using Fluxor;
using Fluxor.Blazor.Web.Components;
using Microsoft.AspNetCore.Components;
using R3;
using RGS.Backend.Shared.Models;
using RGS.Frontend.Store;
using RGS.Frontend.Store.EditSourceResumeDataFeature;

namespace RGS.Frontend.Pages;

public partial class ManageSourceResumeData : FluxorComponent
{
  private enum Section { Bio, Contact, Skills, Projects, Jobs, Education, Books };
  private CompositeDisposable _subscription = new();
  private bool _disposedValue;

  [Inject] private IState<EditSourceResumeDataState> State { get; set; } = null!;
  [Inject] private ILogger<ManageSourceResumeData> Logger { get; set; } = null!;
  [Inject] private IDispatcher Dispatcher { get; set; } = null!;

  protected override async Task OnInitializedAsync()
  {
    await base.OnInitializedAsync();

    Dispatcher.Dispatch(new FetchResumeDataAction());

    var edits = Observable.FromEventHandler(
      a => State.StateChanged += a,
      a => State.StateChanged -= a)
      .Select(e => e.sender as IState<EditSourceResumeDataState>);
    var retries = Observable.Interval(TimeSpan.FromMilliseconds(5000))
      .Select<Unit, IState<EditSourceResumeDataState>?>(_ => State);

    // Submit edits and retries as necessary.
    // TODO: Give up on retries eventually?
    Observable.Merge(edits, retries)
        .WhereNotNull()
        .Where(state => state.Value.ResumeData is not null && state.Value.SaveState.SaveStatus is SaveStatus.Dirty or SaveStatus.Faulted)
        .Debounce(TimeSpan.FromMilliseconds(500))
        .Subscribe(state => HandleSubmit(state.Value.ResumeData!))
        .AddTo(_subscription);
  }

  private void HandleSubmit(SourceResumeData resumeData)
  {
    if (resumeData is not null)
    {
      Dispatcher.Dispatch(new UpdateResumeDataAction(resumeData));
    }
  }

  protected override async ValueTask DisposeAsyncCore(bool disposing)
  {
    await base.DisposeAsyncCore(disposing);

    if (!_disposedValue)
    {
      _subscription.Dispose();
    }

    _disposedValue = true;
  }
}