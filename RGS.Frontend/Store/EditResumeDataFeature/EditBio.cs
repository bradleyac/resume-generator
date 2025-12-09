using System;
using Fluxor;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Store.EditResumeDataFeature;

public record struct UpdateBioAction(Bio Bio);

internal static class BioReducers
{
  [ReducerMethod]
  public static EditResumeDataState UpdateBio(EditResumeDataState state, UpdateBioAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Bio = action.Bio,
      }
    };
  }
}