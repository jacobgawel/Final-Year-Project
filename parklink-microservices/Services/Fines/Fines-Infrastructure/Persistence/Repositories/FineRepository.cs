using System.Globalization;
using AutoMapper;
using Fines_Domain.Data;
using Fines_Domain.Entities;
using Fines_Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace Fines_Infrastructure.Persistence.Repositories;

public class FineRepository : IFineRepository
{
    private readonly FineDbContext _context;
    private readonly IMapper _mapper;
    
    public FineRepository(FineDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    public async Task<List<Fine>> GetFine()
    {
        var fines = await _context.Fines.AsNoTracking().ToListAsync();
        return fines;
    }
    
    public async Task<List<Fine>> GetFinesForProvider(Guid providerId)
    {
        var fines = await _context.Fines.AsNoTracking()
            .Where(p => p.FineIssuerId == providerId)
            .ToListAsync();
        
        return fines;
    }

    public async Task<Fine?> GetFine(Guid id)
    {
        var fine = await _context.Fines
            .AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        
        return fine;
    }
    
    public async Task<Guid> CreateFine(Fine fine)
    {
        var fineId = Guid.NewGuid();
        fine.Id = fineId;
        fine.Total = 50; // standard price for a fine
        fine.FineStatus = false;
        fine.FinePaid = false;
        
        await _context.Fines.AddAsync(fine);
        await _context.SaveChangesAsync();

        return fineId;
    }

    public async Task<bool> UpdateFine(FineUpdateDto fine)
    {
        var existingFine = await _context.Fines.FirstOrDefaultAsync(f => f.Id == fine.Id);

        if (existingFine == null) return false;

        _mapper.Map(fine, existingFine);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteFine(Guid id)
    {
        var fine = await _context.Fines.FirstOrDefaultAsync(p => p.Id == id);

        if (fine == null) return false;
        
        _context.Fines.Remove(fine);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<List<Fine>> GetFineByAccountId(Guid id)
    {
        var fines = await _context.Fines.AsNoTracking()
            .Where(p => p.AccountId == id && p.FineStatus && p.FinePaid == false).ToListAsync();
        return fines;
    }

    public async Task<bool> CheckIfBookingHasFine(Guid id)
    {
        // if the booking already has fines returns true
        var bookings = await _context.Fines.AsNoTracking().Where(p => p.BookingId == id).ToListAsync();
        return bookings.Count != 0;
    }

    public async Task<FineAnalyticsDto> GetFinesAnalyticsForProvider(Guid providerId)
    {
        var fines = await _context.Fines.AsNoTracking()
            .Where(p => p.FineIssuerId == providerId)
            .ToListAsync();

        var pendingFines = fines.Where(p => p is { FineStatus: false, FinePaid: false }).ToList();
        var activeFines = fines.Where(p => p is { FineStatus: true, FinePaid: false }).ToList();
        var paidFines = fines.Where(p => p is { FineStatus: true, FinePaid: true }).ToList();

        var totalFines = paidFines.Sum(t => t.Total);

        var analyticsDto = new FineAnalyticsDto
        {
            ActiveFines = activeFines.Count,
            PendingFines = pendingFines.Count,
            TotalFines = fines.Count,
            PaidFines = paidFines.Count,
            FineRevenue = totalFines.ToString("C", new CultureInfo("en-GB"))
        };

        return analyticsDto;
    }
    
    public async Task<FineAnalyticsDto> GetFinesAnalyticsForAdmin()
    {
        var fines = await _context.Fines.AsNoTracking().ToListAsync();

        var pendingFines = fines.Where(p => p is { FineStatus: false, FinePaid: false }).ToList();
        var activeFines = fines.Where(p => p is { FineStatus: true, FinePaid: false }).ToList();
        var paidFines = fines.Where(p => p is { FineStatus: true, FinePaid: true }).ToList();

        var totalFines = paidFines.Sum(t => t.Total);

        var analyticsDto = new FineAnalyticsDto
        {
            ActiveFines = activeFines.Count,
            PendingFines = pendingFines.Count,
            TotalFines = fines.Count,
            PaidFines = paidFines.Count,
            FineRevenue = totalFines.ToString("C", new CultureInfo("en-GB"))
        };

        return analyticsDto;
    }
}