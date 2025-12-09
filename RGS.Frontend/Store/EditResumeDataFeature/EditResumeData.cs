using System;
using System.Transactions;
using Fluxor;
using Microsoft.Azure.Cosmos.Core;
using Microsoft.Azure.Cosmos.Linq;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Store.EditResumeDataFeature;

public record struct FetchResumeDataAction;
public record struct FetchResumeDataResultAction(ResumeData ResumeData);
public record struct UpdateResumeDataAction(ResumeData ResumeData);
public record struct UpdateResumeDataResultAction(bool Success, string? Message);
public record struct SetSaveStateAction(SaveState SaveState);

internal static class Reducers
{
  [ReducerMethod]
  public static EditResumeDataState SetResumeData(EditResumeDataState state, FetchResumeDataResultAction action) => state with { SaveState = SaveState.Clean, ResumeData = action.ResumeData };
  [ReducerMethod]
  public static EditResumeDataState SetSaveState(EditResumeDataState state, UpdateResumeDataResultAction action) => state with { SaveState = action.Success ? SaveState.Clean : SaveState.FromFault(action.Message) };
}

internal class Effects(IResumeDataService resumeDataService)
{
  private readonly IResumeDataService _resumeDataService = resumeDataService;

  [EffectMethod(typeof(FetchResumeDataAction))]
  public async Task FetchResumeData(IDispatcher dispatcher)
  {
    var resumeData = await _resumeDataService.GetMasterResumeDataAsync();

    if (resumeData is not null)
    {
      dispatcher.Dispatch(new FetchResumeDataResultAction(resumeData));
    }
  }

  [EffectMethod]
  public async Task UpdateResumeData(UpdateResumeDataAction action, IDispatcher dispatcher)
  {
    try
    {
      await _resumeDataService.SetMasterResumeDataAsync(action.ResumeData);
      dispatcher.Dispatch(new UpdateResumeDataResultAction(true, null));
    }
    catch (Exception ex)
    {
      dispatcher.Dispatch(new UpdateResumeDataResultAction(false, ex.Message));
    }
  }
}