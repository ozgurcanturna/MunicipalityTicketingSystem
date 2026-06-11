using Microsoft.AspNetCore.Mvc;

namespace Journey.Telemetry.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private static readonly List<string> Trips = new() { "Trip1", "Trip2", "Trip3" };

    [HttpGet]
    public IActionResult GetTrips()
    {
        return Ok(Trips);
    }

    [HttpGet("{id}")]
    public IActionResult GetTrip(string id)
    {
        return Ok($"Trip {id}");
    }
}
