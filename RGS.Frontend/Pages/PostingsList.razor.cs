using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using R3;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Pages
{
  public partial class PostingsList : ComponentBase
  {
    [Inject] private IPostingsService PostingsService { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    private List<PostingSummary>? Postings { get; set; } = null;
    private Modal Modal { get; set; } = null!;
    private Func<Task>? LoadMorePostings { get; set; } = null;
    private Subject<string> SearchTermSubject { get; set; } = new();
    private IDisposable? _searchTermSubscription;
    private bool _loading;

    [Parameter]
    [SupplyParameterFromQuery]
    public string? Status { get; set; }

    private Task OnAddPostingClicked() => Modal.ShowAsync();

    private Task OnShowMoreClicked() => LoadMorePostings?.Invoke() ?? Task.CompletedTask;

    protected override async Task OnParametersSetAsync()
    {
      _searchTermSubscription?.Dispose();
      _searchTermSubscription = SearchTermSubject
        .Debounce(TimeSpan.FromMilliseconds(300))
        .DistinctUntilChanged()
        .Subscribe(async searchTerm =>
        {
          await LoadPostingsByStatus(searchTerm);
          StateHasChanged();
        });

      await LoadPostingsByStatus();
    }

    private void OnStatusSelected()
    {
      NavigationManager.NavigateTo($"/?Status={Status}");
    }

    private async Task LoadPostingsByStatus(string searchText = "")
    {
      Postings = [];

      var postingsStream = PostingsService.GetPostingsStreamAsync(Status ?? "All", searchText);
      var enumerator = postingsStream.GetAsyncEnumerator();

      LoadMorePostings = async () =>
      {
        if (_loading) return;
        try
        {
          _loading = true;
          if (await enumerator.MoveNextAsync())
          {
            Postings.AddRange(enumerator.Current);
          }
        }
        finally
        {
          _loading = false;
        }
      };

      await LoadMorePostings();
    }

    private async Task OnPostingApplied(string postingId)
    {
      await PostingsService.SetPostingStatusAsync(new(postingId, PostingStatus.Applied));
      Postings = Postings?.Select(p => p.id == postingId ? p with { Status = PostingStatus.Applied } : p).ToList();
    }

    private async Task OnPostingReplied(string postingId)
    {
      await PostingsService.SetPostingStatusAsync(new(postingId, PostingStatus.Replied));
      Postings = Postings?.Select(p => p.id == postingId ? p with { Status = PostingStatus.Replied } : p).ToList();
    }

    private async Task OnPostingArchived(string postingId)
    {
      await PostingsService.SetPostingStatusAsync(new(postingId, PostingStatus.Archived));
      Postings = Postings?.Select(p => p.id == postingId ? p with { Status = PostingStatus.Archived } : p).ToList();
    }

    // TODO: Is this necessary?
    private async Task OnResubmitPosting(string postingId)
    {
      await PostingsService.SetPostingStatusAsync(new(postingId, PostingStatus.Pending));
      Postings = Postings?.Select(p => p.id == postingId ? p with { Status = PostingStatus.Pending } : p).ToList();
    }

    private async Task OnDeletePosting(string postingId)
    {
      await PostingsService.DeletePostingAsync(postingId);
      Postings = Postings?.Where(p => p.id != postingId).ToList();
    }
  }
}