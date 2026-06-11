using Microsoft.AspNetCore.Mvc;

namespace Tenant.Identity.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private static readonly List<string> Users = new() { "User1", "User2", "User3" };

    [HttpGet]
    public IActionResult GetUsers()
    {
        return Ok(Users);
    }

    [HttpGet("{id}")]
    public IActionResult GetUser(string id)
    {
        return Ok($"User {id}");
    }
}
