using apbd_s34366_cw11.Services;
using Microsoft.AspNetCore.Mvc;

namespace apbd_s34366_cw11.Controllers;

[ApiController]
[Route("api/patients")]
public class DbController : ControllerBase
{
    private readonly IDbService _dbService;

    public DbController(IDbService dbService)
    {
        _dbService = dbService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPatients([FromQuery] string? search)
    {
        var patients = await _dbService.GetPatientsAsync(search);
        return Ok(patients);
    }
}