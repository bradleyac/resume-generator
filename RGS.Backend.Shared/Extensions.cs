using System;
using Microsoft.Azure.Cosmos;

namespace RGS.Backend.Shared;

public static class Extensions
{
  public static async Task<IEnumerable<T>> ToListAsync<T>(this FeedIterator<T> @this)
  {
    List<T> results = [];

    while (@this.HasMoreResults)
    {
      var next = await @this.ReadNextAsync();
      results.AddRange(next);
    }

    return results;
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
