using Fluxor;
using RGS.Backend.Shared.Models;

namespace RGS.Frontend.Store.ViewPostingFeature;

[FeatureState]
public record ViewPostingState(SaveState SaveState, JobPosting? Posting = null)
{
  public ViewPostingState()
  : this(SaveState.Clean) { }
}
