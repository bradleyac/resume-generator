using System;
using Fluxor;
using RGS.Backend.Shared.Models;
using RGS.Frontend.Store.EditResumeDataFeature;

namespace RGS.Frontend.Store.EditResumeDataFeature;

public record struct RemoveProjectAction(int ProjectIndex);
public record struct AddProjectAction();
public record struct UpdateProjectAction(int ProjectIndex, Project Project);
public record struct AddTechAction(int ProjectIndex, string Tech);
public record struct RemoveTechAction(int ProjectIndex, int TechIndex);

internal static class ProjectReducers
{
  [ReducerMethod]
  public static EditResumeDataState RemoveProject(EditResumeDataState state, RemoveProjectAction action)
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
  public static EditResumeDataState AddProject(EditResumeDataState state, AddProjectAction action)
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
  public static EditResumeDataState UpdateProject(EditResumeDataState state, UpdateProjectAction action)
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
  public static EditResumeDataState AddTech(EditResumeDataState state, AddTechAction action)
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
  public static EditResumeDataState RemoveTech(EditResumeDataState state, RemoveTechAction action)
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