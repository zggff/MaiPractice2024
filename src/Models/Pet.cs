using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Zggff.MaiPractice;

public class Pet
{
    // id in the database
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public uint Id { get; set; }
    public string? Name { get; set; }
    public string? Species { get; set; }
    public string[]? PhotoUrls { get; set; }

    public PetStatus Status { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PetStatus
{
    Available,
    Pending,
    Sold,
}