using Microsoft.AspNetCore.Mvc;

namespace Zggff.MaiPractice.Controllers;

[ApiController]
public class SomeApiController
{
    [HttpPost(nameof(GetId))]
    public int GetId(int id)
    {
        return id;
    }
}