using System;
using Fluxor;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Store.ViewPostingFeature;

public record struct SaveNotesAction(Notes Notes);
public record struct SaveNotesResultAction(bool Success, Notes? Notes, string? Message);
public record struct EditNotesAction(Notes Notes);

internal static class NotesReducers
{
  [ReducerMethod]
  public static ViewPostingState SetNotes(ViewPostingState state, EditNotesAction action) => state with
  {
    Posting = state.Posting is null ? null : state.Posting with { Notes = action.Notes },
    SaveState = SaveState.Dirty,
  };

  [ReducerMethod]
  public static ViewPostingState SetNotes(ViewPostingState state, SaveNotesResultAction action) => state with
  {
    SaveState = action.Success ? SaveState.Clean : SaveState.FromFault(action.Message),
    Posting = state.Posting is null ? null : state.Posting with { Notes = action.Notes },
  };
}

internal class ViewPostingEffects(IPostingsService postingsService)
{

  [EffectMethod]
  public async Task UpdateNotes(SaveNotesAction action, IDispatcher dispatcher)
  {
    try
    {
      await postingsService.SetPostingNotes(action.Notes);
      dispatcher.Dispatch(new SaveNotesResultAction(true, action.Notes, null));
    }
    catch (Exception ex)
    {
      dispatcher.Dispatch(new SaveNotesResultAction(false, null, ex.Message));
    }
  }
}