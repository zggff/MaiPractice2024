using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Zggff.MaiPractice;

public class User
{
    // id in the database
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public uint Id { get; set; }
    [Required(ErrorMessage = "Login is required")]
    public required string Login { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
    public Role Role { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Role
{
    User,
    Admin,

}