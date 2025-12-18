using System;
using System.Transactions;
using Fluxor;
using Microsoft.Azure.Cosmos.Core;
using Microsoft.Azure.Cosmos.Linq;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Store.EditSourceResumeDataFeature;

public record struct FetchResumeDataAction;
public record struct FetchResumeDataResultAction(SourceResumeData ResumeData);
public record struct UpdateResumeDataAction(SourceResumeData ResumeData);
public record struct UpdateResumeDataResultAction(bool Success, string? Message);
public record struct SetSaveStateAction(SaveState SaveState);

internal static class Reducers
{
  [ReducerMethod]
  public static EditSourceResumeDataState SetResumeData(EditSourceResumeDataState state, FetchResumeDataResultAction action) => state with { SaveState = SaveState.Clean, ResumeData = action.ResumeData };
  [ReducerMethod]
  public static EditSourceResumeDataState SetSaveState(EditSourceResumeDataState state, UpdateResumeDataResultAction action) => state with { SaveState = action.Success ? SaveState.Clean : SaveState.FromFault(action.Message) };
}

internal class EditSourceResumeDataEffects(IResumeDataService resumeDataService)
{
  private readonly IResumeDataService _resumeDataService = resumeDataService;

  [EffectMethod(typeof(FetchResumeDataAction))]
  public async Task FetchResumeData(IDispatcher dispatcher)
  {
    try
    {
      var resumeData = await _resumeDataService.GetSourceResumeDataAsync();

      if (resumeData is not null)
      {
        dispatcher.Dispatch(new FetchResumeDataResultAction(resumeData));
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
    }
  }

  [EffectMethod]
  public async Task UpdateResumeData(UpdateResumeDataAction action, IDispatcher dispatcher)
  {
    try
    {
      await _resumeDataService.SetSourceResumeDataAsync(action.ResumeData);
      dispatcher.Dispatch(new UpdateResumeDataResultAction(true, null));
    }
    catch (Exception ex)
    {
      dispatcher.Dispatch(new UpdateResumeDataResultAction(false, ex.Message));
    }
  }
}