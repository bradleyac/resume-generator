using System;
using Fluxor;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Store.EditSourceResumeDataFeature;

public record struct RemoveEducationAction(int EducationIndex);
public record struct AddEducationAction();
public record struct UpdateEducationAction(int EducationIndex, Education Education);

internal static class EducationReducers
{
  [ReducerMethod]
  public static EditSourceResumeDataState RemoveEducation(EditSourceResumeDataState state, RemoveEducationAction action)
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
  public static EditSourceResumeDataState AddEducation(EditSourceResumeDataState state, AddEducationAction action)
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
  public static EditSourceResumeDataState UpdateEducation(EditSourceResumeDataState state, UpdateEducationAction action)
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