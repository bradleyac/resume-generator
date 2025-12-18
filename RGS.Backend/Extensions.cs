using System;
using Microsoft.Azure.Cosmos;

namespace RGS.Backend;

public static class Extensions
{
  public static async Task<List<T>> ToListAsync<T>(this FeedIterator<T> @this)
  {
    List<T> results = [];

    while (@this.HasMoreResults)
    {
      var next = await @this.ReadNextAsync();
      if (next.StatusCode != System.Net.HttpStatusCode.OK)
      {
        throw new RGSException($"FeedIterator returned non-OK status code: {next.StatusCode}");
      }
      results.AddRange(next);
    }

    return results;
  }

  public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this FeedIterator<T> @this)
  {
    while (@this.HasMoreResults)
    {
      var next = await @this.ReadNextAsync();

      if (next.StatusCode != System.Net.HttpStatusCode.OK)
      {
        throw new RGSException($"FeedIterator returned non-OK status code: {next.StatusCode}");
      }

      foreach (var item in next)
      {
        yield return item;
      }
    }
  }

  /// <summary>
  /// Like Aggregate, but emits the accumulator at each step.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="this"></param>
  /// <param name="func"></param>
  /// <returns></returns>
  public static IEnumerable<T> Scan<T>(this IEnumerable<T> @this, Func<T, T, T> func)
  {
    T? acc = default;
    foreach (var val in @this)
    {
      acc = acc is null ? val : func(acc, val);
      yield return acc;
    }
  }
}
