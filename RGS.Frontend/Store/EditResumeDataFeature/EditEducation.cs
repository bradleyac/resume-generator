using System;
using Fluxor;
using RGS.Backend.Shared.Models;
using RGS.Frontend.Store.EditResumeDataFeature;

namespace RGS.Frontend.Store.EditResumeDataFeature;

public record struct RemoveEducationAction(int EducationIndex);
public record struct AddEducationAction();
public record struct UpdateEducationAction(int EducationIndex, Education Education);

internal static class EducationReducers
{
  [ReducerMethod]
  public static EditResumeDataState RemoveEducation(EditResumeDataState state, RemoveEducationAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Education = [.. state.ResumeData.Education.RemoveAt(action.EducationIndex)]
      }
    };
  }

  [ReducerMethod]
  public static EditResumeDataState AddEducation(EditResumeDataState state, AddEducationAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Education = [.. state.ResumeData.Education, new("", "", "", "")],
      }
    };
  }

  [ReducerMethod]
  public static EditResumeDataState UpdateEducation(EditResumeDataState state, UpdateEducationAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Education = [.. state.ResumeData.Education.ReplaceAt(action.EducationIndex, _ => action.Education)],
      }
    };
  }
}