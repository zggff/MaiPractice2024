using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Zggff.MaiPractice;

public class User
{
    // id in the database
    [ScaffoldColumn(false)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public uint Id { get; set; }
    [Required(ErrorMessage = "Login is required")]
    public string? Login { get; set; }
    [Required(ErrorMessage = "Password is required")]
    public string? Password { get; set; }
    public Role Role { get; set; }
}

public enum Role
{
    User,
    Admin,

}