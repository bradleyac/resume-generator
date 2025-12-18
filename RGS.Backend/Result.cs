using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace RGS.Backend;

internal class Result
{
  public bool IsSuccess { get; }
  public HttpStatusCode? StatusCode { get; init; }
  public string? ErrorMessage { get; }

  protected Result(bool isSuccess, string? errorMessage, HttpStatusCode? statusCode = null)
  {
    IsSuccess = isSuccess;
    ErrorMessage = errorMessage;
    StatusCode = statusCode;
  }

  public static Result Success() => new Result(true, null);

  public static Result Failure(string errorMessage, HttpStatusCode statusCode) => new Result(false, errorMessage, statusCode);
}

internal class Result<T> : Result
{
  public T? Value { get; }

  protected Result(bool isSuccess, T? value, string? errorMessage, HttpStatusCode? statusCode = null) : base(isSuccess, errorMessage, statusCode)
  {
    Value = value;
  }

  public static Result<T> Success(T value) => new Result<T>(true, value, null);

  public new static Result<T> Failure(string errorMessage, HttpStatusCode statusCode) => new Result<T>(false, default, errorMessage, statusCode);
}

internal static class ResultExtensions
{
  public static Result<TNew> ConvertFailure<TOld, TNew>(this Result<TOld> result)
  {
    if (result.IsSuccess)
    {
      throw new InvalidOperationException("Cannot convert a successful Result to Result<T>");
    }

    return Result<TNew>.Failure(result.ErrorMessage ?? "Unknown error", result.StatusCode ?? HttpStatusCode.InternalServerError);
  }

  /// <summary>
  /// Keeps NotFound, otherwise maps to InternalServerError
  /// </summary>
  /// <param name="statusCode"></param>
  /// <returns></returns>
  public static HttpStatusCode FromCosmosDBStatusCode(this HttpStatusCode statusCode) => statusCode switch
  {
    HttpStatusCode.NotFound => HttpStatusCode.NotFound,
    _ => HttpStatusCode.InternalServerError,
  };

  public static IActionResult ToJsonActionResult<T>(this Result<T> result)
  {
    return result switch
    {
      { IsSuccess: true } => new JsonResult(result.Value!),
      var failure => new ObjectResult(new ProblemDetails
      {
        Title = "Operation failed",
        Status = (int)(failure.StatusCode ?? HttpStatusCode.InternalServerError),
        Detail = failure.ErrorMessage,
      })
      {
        StatusCode = (int)(failure.StatusCode ?? HttpStatusCode.InternalServerError)
      }
    };
  }

  public static IActionResult ToActionResult(this Result result)
  {
    return result switch
    {
      { IsSuccess: true } => new OkResult(),
      var failure => new ObjectResult(new ProblemDetails
      {
        Title = "Operation failed",
        Status = (int)(failure.StatusCode ?? HttpStatusCode.InternalServerError),
        Detail = failure.ErrorMessage,
      })
      {
        StatusCode = (int)(failure.StatusCode ?? HttpStatusCode.InternalServerError)
      }
    };
  }
}