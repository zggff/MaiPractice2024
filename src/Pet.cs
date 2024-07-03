using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Zggff.MaiPractice;

public class Pet
{
    // id in the database
    public uint Id { get; set; }
    public string? Name { get; set; }
    public string? Species { get; set; }
    public string[]? PhotoUrls { get; set; }

    public Status Status { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status
{
    Available,
    Pending,
    Sold,
}