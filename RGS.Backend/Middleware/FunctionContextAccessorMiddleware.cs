using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using RGS.Backend.Services;

namespace RGS.Backend.Middleware;

internal class FunctionContextAccessorMiddleware(FunctionContextAccessor functionContextAccessor) : IFunctionsWorkerMiddleware
{
  private readonly FunctionContextAccessor _functionContextAccessor = functionContextAccessor;

  public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
  {
    _functionContextAccessor.Current = context;
    await next(context);
  }
}