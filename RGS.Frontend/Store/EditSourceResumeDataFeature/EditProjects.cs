using System;
using Fluxor;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Store.EditSourceResumeDataFeature;

public record struct RemoveProjectAction(int ProjectIndex);
public record struct AddProjectAction();
public record struct UpdateProjectAction(int ProjectIndex, Project Project);
public record struct AddTechAction(int ProjectIndex, string Tech);
public record struct RemoveTechAction(int ProjectIndex, int TechIndex);

internal static class ProjectReducers
{
  [ReducerMethod]
  public static EditSourceResumeDataState RemoveProject(EditSourceResumeDataState state, RemoveProjectAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Projects = [.. state.ResumeData.Projects.RemoveAt(action.ProjectIndex)]
      }
    };
  }

  [ReducerMethod]
  public static EditSourceResumeDataState AddProject(EditSourceResumeDataState state, AddProjectAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Projects = [.. state.ResumeData.Projects, new("", "", [], "")]
      }
    };
  }

  [ReducerMethod]
  public static EditSourceResumeDataState UpdateProject(EditSourceResumeDataState state, UpdateProjectAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Projects = [.. state.ResumeData.Projects.ReplaceAt(action.ProjectIndex, _ => action.Project)]
      }
    };
  }

  [ReducerMethod]
  public static EditSourceResumeDataState AddTech(EditSourceResumeDataState state, AddTechAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Projects = [.. state.ResumeData.Projects.ReplaceAt(action.ProjectIndex, project => project with { Technologies = [.. project.Technologies, action.Tech] })]
      }
    };
  }

  [ReducerMethod]
  public static EditSourceResumeDataState RemoveTech(EditSourceResumeDataState state, RemoveTechAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Projects = [.. state.ResumeData.Projects.ReplaceAt(action.ProjectIndex, project =>
        project with { Technologies = [.. project.Technologies.RemoveAt(action.TechIndex)]})]
      }
    };
  }
}