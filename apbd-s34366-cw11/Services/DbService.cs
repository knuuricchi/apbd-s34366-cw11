using apbd_s34366_cw11.Data;
using apbd_s34366_cw11.DTOs;
using apbd_s34366_cw11.Models;
using Microsoft.EntityFrameworkCore;

namespace apbd_s34366_cw11.Services;

public class DbService : IDbService
{
    private readonly HospitalContext _context;

    public DbService(HospitalContext context)
    {
        _context = context;
    }

    public async Task<List<PatientDetailsResponseDto>> GetPatientsAsync(string? search)
    {
        var query = _context.Patients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search}%";
            query = query.Where(p =>
                EF.Functions.Like(p.FirstName, pattern) ||
                EF.Functions.Like(p.LastName, pattern));
        }

        return await query
            .Select(p => new PatientDetailsResponseDto
            {
                Pesel = p.Pesel,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Age = p.Age,
                Sex = p.Sex ? "Male" : "Female",
                Admissions = p.Admissions.Select(a => new AdmissionDto
                {
                    Id = a.Id,
                    AdmissionDate = a.AdmissionDate,
                    DischargeDate = a.DischargeDate,
                    Ward = new WardDto
                    {
                        Id = a.Ward.Id,
                        Name = a.Ward.Name,
                        Description = a.Ward.Description
                    }
                }).ToList(),
                BedAssignments = p.BedAssignments.Select(ba => new BedAssignmentDto
                {
                    Id = ba.Id,
                    From = ba.From,
                    To = ba.To,
                    Bed = new BedDto
                    {
                        Id = ba.Bed.Id,
                        BedType = new BedTypeDto
                        {
                            Id = ba.Bed.BedType.Id,
                            Name = ba.Bed.BedType.Name,
                            Description = ba.Bed.BedType.Description
                        },
                        Room = new RoomDto
                        {
                            Id = ba.Bed.Room.Id,
                            HasTv = ba.Bed.Room.HasTv,
                            Ward = new WardDto
                            {
                                Id = ba.Bed.Room.Ward.Id,
                                Name = ba.Bed.Room.Ward.Name,
                                Description = ba.Bed.Room.Ward.Description
                            }
                        }
                    }
                }).ToList()
            })
            .ToListAsync();
    }
    
    public async Task AssignBedAsync(string pesel, BedAssignmentRequestDto dto)
    {
        var patientExists = await _context.Patients.AnyAsync(p => p.Pesel == pesel);
        if (!patientExists)
        {
            throw new KeyNotFoundException($"Patient with PESEL '{pesel}' does not exist.");
        }

        var ward = await _context.Wards.FirstOrDefaultAsync(w => w.Name == dto.Ward);
        if (ward == null)
        {
            throw new KeyNotFoundException($"Ward '{dto.Ward}' does not exist.");
        }

        var bedType = await _context.BedTypes.FirstOrDefaultAsync(bt => bt.Name == dto.BedType);
        if (bedType == null)
        {
            throw new KeyNotFoundException($"Bed type '{dto.BedType}' does not exist.");
        }

        var maxSqlDate = new DateTime(3000, 1, 1);

        var reqStart = dto.From;
        var reqEnd = dto.To ?? maxSqlDate;

        var availableBed = await _context.Beds
            .Where(b => b.Room.WardId == ward.Id && b.BedTypeId == bedType.Id)
            .Where(b => !_context.BedAssignments.Any(ba =>
                ba.BedId == b.Id &&
                reqStart < (ba.To ?? maxSqlDate) &&
                reqEnd > ba.From))
            .FirstOrDefaultAsync();

        if (availableBed == null)
        {
            throw new InvalidOperationException($"No available bed of type '{dto.BedType}' found in ward '{dto.Ward}' for the requested period.");
        }

        var newAssignment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = availableBed.Id,
            From = dto.From,
            To = dto.To
        };

        _context.BedAssignments.Add(newAssignment);
        await _context.SaveChangesAsync();
    }
}