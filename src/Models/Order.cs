using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Zggff.MaiPractice;

public class Order
{
    // id in the database
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public uint Id { get; set; }
    public uint PetId { get; set; }
    public uint UserId { get; set; }

    public DateTime Placed { get; set; }
    public OrderStatus Status { get; set; }

}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrderStatus
{
    Placed,
    Approved,
    Completed
}