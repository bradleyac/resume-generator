using System;
using Fluxor;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Store.EditSourceResumeDataFeature;

public record struct UpdateContactAction(Contact Contact);

internal static class ContactReducers
{
  [ReducerMethod]
  public static EditSourceResumeDataState UpdateContact(EditSourceResumeDataState state, UpdateContactAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Contact = action.Contact,
      }
    };
  }
}