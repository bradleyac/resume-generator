using System;
using Fluxor;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Store.EditResumeDataFeature;

public record struct RemoveSkillAction(int SkillCategoryIndex, int SkillIndex);
public record struct AddSkillAction(int SkillCategoryIndex, string Skill);
public record struct UpdateSkillCategoryAction(int SkillCategoryIndex, SkillCategory SkillCategory);
public record struct RemoveSkillCategoryAction(int SkillCategoryIndex);
public record struct AddSkillCategoryAction(string Label);

public static class SkillReducers
{
  [ReducerMethod]
  public static EditResumeDataState RemoveSkill(EditResumeDataState state, RemoveSkillAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Skills = [.. state.ResumeData.Skills.ReplaceAt(action.SkillCategoryIndex, cat => cat with { Items = [.. cat.Items.RemoveAt(action.SkillIndex)] })]
      }
    };
  }

  [ReducerMethod]
  public static EditResumeDataState AddSkill(EditResumeDataState state, AddSkillAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Skills = [.. state.ResumeData.Skills.ReplaceAt(action.SkillCategoryIndex, cat => cat with { Items = [.. cat.Items, action.Skill] })]
      }
    };
  }

  [ReducerMethod]
  public static EditResumeDataState UpdateSkillCategory(EditResumeDataState state, UpdateSkillCategoryAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Skills = [.. state.ResumeData.Skills.ReplaceAt(action.SkillCategoryIndex, _ => action.SkillCategory)]
      }
    };
  }

  [ReducerMethod]
  public static EditResumeDataState RemoveSkillCategory(EditResumeDataState state, RemoveSkillCategoryAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Skills = [.. state.ResumeData.Skills.RemoveAt(action.SkillCategoryIndex)]
      }
    };
  }

  [ReducerMethod]
  public static EditResumeDataState AddSkillCategory(EditResumeDataState state, AddSkillCategoryAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Skills = [.. state.ResumeData.Skills, new SkillCategory(action.Label, [])]
      }
    };
  }
}