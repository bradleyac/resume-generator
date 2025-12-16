using Microsoft.Azure.Cosmos;
using RGS.Backend.Shared.Models;
using System.IO;
using System.Text;
using System.Text.Json;

namespace RGS.Backend;

public class CosmosSystemTextJsonSerializer : CosmosSerializer
{
  private readonly JsonSerializerOptions _jsonSerializerOptions;

  public CosmosSystemTextJsonSerializer()
  {
    _jsonSerializerOptions = new JsonSerializerOptions
    {
      WriteIndented = false,
      AllowOutOfOrderMetadataProperties = true,
    };
  }

  public override T FromStream<T>(Stream stream)
  {
    using (stream)
    {
      if (stream.CanSeek && stream.Length == 0) return default!;
      if (typeof(T) == typeof(Stream)) return (T)(object)stream;

      return JsonSerializer.Deserialize<T>(stream, _jsonSerializerOptions)!;
    }
  }

  public override Stream ToStream<T>(T input)
  {
    var stream = new MemoryStream();
    JsonSerializer.Serialize(stream, input, typeof(T), _jsonSerializerOptions);
    stream.Position = 0;
    return stream;
  }
}