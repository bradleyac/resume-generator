using Fluxor;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Store.EditResumeDataFeature;

[FeatureState]
public record EditResumeDataState(SaveState SaveState, SourceResumeData? ResumeData = null)
{
  public EditResumeDataState()
  : this(SaveState.Clean) { }
}
