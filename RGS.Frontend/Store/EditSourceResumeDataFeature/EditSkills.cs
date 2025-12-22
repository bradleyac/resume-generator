using System;
using Fluxor;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Store.EditSourceResumeDataFeature;

public record struct RemoveSkillAction(int SkillCategoryIndex, int SkillIndex);
public record struct AddSkillAction(int SkillCategoryIndex, string Skill);
public record struct UpdateSkillCategoryAction(int SkillCategoryIndex, SkillCategory SkillCategory);
public record struct RemoveSkillCategoryAction(int SkillCategoryIndex);
public record struct AddSkillCategoryAction(string Label);

public static class SkillReducers
{
  [ReducerMethod]
  public static EditSourceResumeDataState RemoveSkill(EditSourceResumeDataState state, RemoveSkillAction action)
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
  public static EditSourceResumeDataState AddSkill(EditSourceResumeDataState state, AddSkillAction action)
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
  public static EditSourceResumeDataState UpdateSkillCategory(EditSourceResumeDataState state, UpdateSkillCategoryAction action)
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
  public static EditSourceResumeDataState RemoveSkillCategory(EditSourceResumeDataState state, RemoveSkillCategoryAction action)
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
  public static EditSourceResumeDataState AddSkillCategory(EditSourceResumeDataState state, AddSkillCategoryAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Skills = [.. state.ResumeData.Skills, new SkillCategory(Guid.NewGuid().ToString(), action.Label, [])]
      }
    };
  }
}