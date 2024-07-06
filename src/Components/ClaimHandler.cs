using System.Security.Claims;
using Zggff.MaiPractice.Models;

namespace Zggff.MaiPractice.Components;


public class ClaimHandler(IHttpContextAccessor accessor)
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

    public UserRole Role()
    {
        var res = Enum.TryParse(Claim(ClaimTypes.Role) ?? "User", out UserRole role);
        if (res)
            return role;
        return UserRole.User;
    }
}