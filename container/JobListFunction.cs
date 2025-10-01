using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace RGS.Functions;

public class JobListFunction
{
    private readonly ILogger<JobListFunction> _logger;

    public JobListFunction(ILogger<JobListFunction> logger)
    {
        _logger = logger;
    }

    [Function("JobListFunction")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Welcome to Azure Functions!");
    }
}