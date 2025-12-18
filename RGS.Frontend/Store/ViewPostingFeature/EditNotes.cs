using System;
using Fluxor;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Store.ViewPostingFeature;

public record struct EditNotesAction(Notes Notes);
public record struct EditNotesResultAction(bool Success, Notes? Notes, string? Message);

internal static class NotesReducers
{
  [ReducerMethod]
  public static ViewPostingState SetNotes(ViewPostingState state, EditNotesResultAction action) => state with
  {
    SaveState = action.Success ? SaveState.Clean : SaveState.FromFault(action.Message),
    Posting = state.Posting is null ? null : state.Posting with { Notes = action.Notes },
  };
}

internal class ViewPostingEffects(IPostingsService postingsService)
{

  [EffectMethod]
  public async Task UpdateNotes(EditNotesAction action, IDispatcher dispatcher)
  {
    try
    {
      await postingsService.SetPostingNotes(action.Notes);
      dispatcher.Dispatch(new EditNotesResultAction(true, action.Notes, null));
    }
    catch (Exception ex)
    {
      dispatcher.Dispatch(new EditNotesResultAction(false, null, ex.Message));
    }
  }
}