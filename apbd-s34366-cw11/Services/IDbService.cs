using apbd_s34366_cw11.DTOs;

namespace apbd_s34366_cw11.Services;

public interface IDbService
{
    Task<List<PatientDetailsResponseDto>> GetPatientsAsync(string? search);
    Task AssignBedAsync(string pesel, BedAssignmentRequestDto dto);
}