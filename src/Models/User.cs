using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Zggff.MaiPractice.Models;

public class User
{
    // id in the database
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public uint Id { get; set; }
    [Required(ErrorMessage = "Login is required")]
    public required string Login { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
    public UserRole Role { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    User,
    Admin,

}