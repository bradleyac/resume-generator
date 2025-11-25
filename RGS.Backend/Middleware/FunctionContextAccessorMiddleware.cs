using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using RGS.Backend.Services;

namespace RGS.Backend.Middleware;

internal class FunctionContextAccessorMiddleware : IFunctionsWorkerMiddleware
{
  public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
  {
    var scopedContextAccessor = context.InstanceServices.GetRequiredService<FunctionContextAccessor>();
    scopedContextAccessor.Current = context;
    await next(context);
  }
}