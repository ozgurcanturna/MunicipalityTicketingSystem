using Microsoft.AspNetCore.Mvc;

namespace Ticketing.Wallet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WalletsController : ControllerBase
{
    private static readonly List<string> Wallets = new() { "Wallet1", "Wallet2", "Wallet3" };

    [HttpGet]
    public IActionResult GetWallets()
    {
        return Ok(Wallets);
    }

    [HttpGet("{id}")]
    public IActionResult GetWallet(string id)
    {
        return Ok($"Wallet {id}");
    }
}
