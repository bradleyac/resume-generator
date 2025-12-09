using System;
using Fluxor;
using RGS.Backend.Shared.Models;
using RGS.Frontend.Store.EditResumeDataFeature;

namespace RGS.Frontend.Store.EditResumeDataFeature;

public record struct RemoveJobAction(int JobIndex);
public record struct AddJobAction();
public record struct UpdateJobAction(int JobIndex, Job Job);
public record struct AddBulletAction(int JobIndex, string Bullet);
public record struct RemoveBulletAction(int JobIndex, int BulletIndex);

internal static class JobReducers
{
  [ReducerMethod]
  public static EditResumeDataState RemoveJob(EditResumeDataState state, RemoveJobAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Jobs = [.. state.ResumeData.Jobs.RemoveAt(action.JobIndex)]
      }
    };
  }

  [ReducerMethod]
  public static EditResumeDataState AddJob(EditResumeDataState state, AddJobAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Jobs = [.. state.ResumeData.Jobs, new("", "", "", "", "", [])],
      }
    };
  }

  [ReducerMethod]
  public static EditResumeDataState UpdateJob(EditResumeDataState state, UpdateJobAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Jobs = [.. state.ResumeData.Jobs.ReplaceAt(action.JobIndex, _ => action.Job)],
      }
    };
  }

  [ReducerMethod]
  public static EditResumeDataState AddBullet(EditResumeDataState state, AddBulletAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Jobs = [.. state.ResumeData.Jobs.ReplaceAt(action.JobIndex, job => job with { Bullets = [.. job.Bullets, action.Bullet] })]
      }
    };
  }

  [ReducerMethod]
  public static EditResumeDataState RemoveBullet(EditResumeDataState state, RemoveBulletAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Jobs = [.. state.ResumeData.Jobs.ReplaceAt(action.JobIndex, job =>
          job with { Bullets = [.. job.Bullets.RemoveAt(action.BulletIndex)]})]
      }
    };
  }
}