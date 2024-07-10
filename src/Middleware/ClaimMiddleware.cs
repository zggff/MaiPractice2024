using System.Security.Claims;
using Zggff.MaiPractice.Models;

namespace Zggff.MaiPractice.Middleware;


public class ClaimMiddleware(IHttpContextAccessor accessor)
{
    private IHttpContextAccessor accessor { get; } = accessor;

    public string? Claim(string claim_type)
    {
        return accessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == claim_type)?.Value;
    }

    public string Login()
    {
        return Claim(ClaimTypes.Name) ?? "";
    }
    public uint Id()
    {
        return uint.Parse(Claim(ClaimTypes.NameIdentifier) ?? "0");
    }

    // this funcion should not be used to check if the user is logged in
    public UserRole Role()
    {
        if (Enum.TryParse(Claim(ClaimTypes.Role) ?? "User", out UserRole role))
            return role;
        return UserRole.User; // the default value. 
    }
}