using Fluxor;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Store.EditSourceResumeDataFeature;

[FeatureState]
public record EditSourceResumeDataState(SaveState SaveState, SourceResumeData? ResumeData = null)
{
  public EditSourceResumeDataState()
  : this(SaveState.Clean) { }
}
