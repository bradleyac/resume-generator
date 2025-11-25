using Microsoft.Azure.Functions.Worker;

namespace RGS.Backend.Services;

internal class FunctionContextAccessor
{
  public FunctionContext? Current { get; set; }
}