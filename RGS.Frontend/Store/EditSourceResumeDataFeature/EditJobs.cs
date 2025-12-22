using System;
using Fluxor;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Store.EditSourceResumeDataFeature;

public record struct RemoveJobAction(int JobIndex);
public record struct AddJobAction();
public record struct UpdateJobAction(int JobIndex, Job Job);
public record struct AddBulletAction(int JobIndex, string Bullet);
public record struct RemoveBulletAction(int JobIndex, int BulletIndex);

internal static class JobReducers
{
  [ReducerMethod]
  public static EditSourceResumeDataState RemoveJob(EditSourceResumeDataState state, RemoveJobAction action)
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
  public static EditSourceResumeDataState AddJob(EditSourceResumeDataState state, AddJobAction action)
  {
    if (state.ResumeData is null) return state;

    return state with
    {
      SaveState = SaveState.Dirty,
      ResumeData = state.ResumeData with
      {
        Jobs = [.. state.ResumeData.Jobs, new(Guid.NewGuid().ToString(), "", "", "", "", "", [])],
      }
    };
  }

  [ReducerMethod]
  public static EditSourceResumeDataState UpdateJob(EditSourceResumeDataState state, UpdateJobAction action)
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
  public static EditSourceResumeDataState AddBullet(EditSourceResumeDataState state, AddBulletAction action)
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
  public static EditSourceResumeDataState RemoveBullet(EditSourceResumeDataState state, RemoveBulletAction action)
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