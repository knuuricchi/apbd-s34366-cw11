using apbd_s34366_cw11.DTOs;
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
    
    [HttpPost("{pesel}/bedassignments")]
    public async Task<IActionResult> AssignBed(string pesel, [FromBody] BedAssignmentRequestDto dto)
    {
        try
        {
            await _dbService.AssignBedAsync(pesel, dto);
            return StatusCode(201, new { Message = "Bed successfully assigned." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }
}