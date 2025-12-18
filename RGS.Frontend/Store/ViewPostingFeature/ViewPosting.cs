using System;
using System.Transactions;
using Fluxor;
using Microsoft.Azure.Cosmos.Core;
using Microsoft.Azure.Cosmos.Linq;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Store.ViewPostingFeature;

public record struct FetchPostingAction(string PostingId);
public record struct FetchPostingResultAction(JobPosting Posting);
public record struct SetSaveStateAction(SaveState SaveState);

internal static class Reducers
{
  [ReducerMethod]
  public static ViewPostingState SetPostingData(ViewPostingState state, FetchPostingResultAction action) => state with { SaveState = SaveState.Clean, Posting = action.Posting };
  [ReducerMethod]
  public static ViewPostingState SetSaveState(ViewPostingState state, SetSaveStateAction action) => state with { SaveState = action.SaveState };
}

internal class Effects(IPostingsService postingsService)
{
  private readonly IPostingsService _postingsService = postingsService;

  [EffectMethod]
  public async Task FetchPosting(FetchPostingAction action, IDispatcher dispatcher)
  {
    try
    {
      // Clear current state.
      dispatcher.Dispatch(new FetchPostingResultAction(null!));

      var posting = await _postingsService.GetPostingAsync(action.PostingId);

      if (posting is not null)
      {
        dispatcher.Dispatch(new FetchPostingResultAction(posting));
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
    }
  }
}