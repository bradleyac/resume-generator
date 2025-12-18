using System;

namespace RGS.Frontend.Store;

public record SaveState(SaveStatus SaveStatus, string? Message)
{
  public static SaveState Dirty = new SaveState(SaveStatus.Dirty, null);
  public static SaveState Clean = new SaveState(SaveStatus.Clean, null);
  public static SaveState FromFault(string? fault) => new SaveState(SaveStatus.Faulted, fault ?? "Unknown fault");
};

public enum SaveStatus
{
  Dirty,
  Clean,
  Faulted,
};