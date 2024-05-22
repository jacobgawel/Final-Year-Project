using System.Globalization;
using AutoMapper;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Parking_Domain.Data;
using Parking_Domain.Entities;
using Parking_Infrastructure.Data;

namespace Parking_Infrastructure.Repositories;

public class ParkingRepository : IParkingRepository
{
    private readonly ParkingDbContext _context;
    private readonly IMapper _mapper;
    public ParkingRepository(ParkingDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<Parking>> GetParking()
    {
        // this will return all parking spots
        var parking = await _context.Parking.AsNoTracking().ToListAsync();
        return parking;
    }

    public async Task<List<Parking>> GetParkingByCity(string city)
    {
        var parking = await _context.Parking.AsNoTracking()
            .Where(p => p.AvailabilityStatus == true && p.VerificationStatus == true && p.City == city).ToListAsync();
        return parking;
    }

    public async Task<List<Parking>> GetParkingByVerified()
    {
        // this will return only verified parking spots (verification must be applied by an administrator)
        var parking = await _context.Parking.AsNoTracking()
            .Where(p => p.VerificationStatus == true)
            .ToListAsync();

        var sorted = parking.OrderByDescending(p => p.AvailabilityStatus == true).ToList();
        // parking spots that are available should be displayed first
        return sorted;
    }

    public async Task<List<ParkingDistanceDto>> GetClosestParkingSpots(double lat, double lon, TimeSpan duration)
    {
        // TODO: Add a city or some other type of location to initially query over later on
        
        // Looping through every parking spot now, this is a shitty way to do it, but it will work for now lol
        var parking = await _context.Parking.AsNoTracking()
            .Where(p => p.VerificationStatus == true)
            .ToListAsync();

        var parkingDistances = new List<ParkingDistanceDto>();
        
        parking.ForEach(p =>
        {
            var pLon = (double) p.Longitude!;
            var pLat = (double) p.Latitude!;
            var distance = Geolocation.GeoCalculator.GetDistance(lat, lon, pLat, pLon);
            var parkingDistance = new ParkingDistanceDto
            {
                Parking = p,
                Distance = distance
            };
            parkingDistances.Add(parkingDistance);
        });

        var sorted = parkingDistances.OrderBy(p => p.Distance).ToList();

        return sorted;
    }

    public async Task<Parking?> GetParking(Guid id)
    {
        var parking = await _context.Parking.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return parking;
    }

    public async Task<List<Parking>> GetParkingByProviderId(Guid userId)
    {
        var parking = await _context.Parking.AsNoTracking().Where(p => p.AccountId == userId).ToListAsync();
        var sorted = parking.OrderByDescending(p => p.CreatedAt).ToList();
        return sorted;
    }

    public async Task<Guid> CreateParking(Parking parking)
    {
        var generatedId = Guid.NewGuid();
        var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var isDayLightSaving = info.IsDaylightSavingTime(DateTime.UtcNow);
        
        parking.Id = generatedId;
        parking.CreatedAt = isDayLightSaving ? DateTime.UtcNow.AddHours(1) : DateTime.UtcNow;
        parking.LastEditDate = isDayLightSaving ? DateTime.UtcNow.AddHours(1) : DateTime.UtcNow;
        
        await _context.Parking.AddAsync(parking);
        await _context.SaveChangesAsync();
        
        return generatedId;
    }

    public async Task<bool> UpdateParking(ParkingUpdateDto parking)
    {
        var existingParking = await _context.Parking.FirstOrDefaultAsync
            (p => p.Id == parking.Id);

        if (existingParking == null) return false;

        if (parking.ParkingRejected)
        {
            parking.VerificationStatus = false;
            parking.AvailabilityStatus = false;
        }
        
        var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var isDayLightSaving = info.IsDaylightSavingTime(DateTime.UtcNow);

        if (existingParking.VerificationStatus == false && parking.VerificationStatus)
        {
            existingParking.VerificationDate = isDayLightSaving ? DateTime.UtcNow.AddHours(1) : DateTime.UtcNow;
        }

        if (existingParking.Address != parking.Address)
        {
            // Assigning the verification status to the new
            // parking instance since it will be mapped in the next step
            // source -> destination
            parking.VerificationStatus = false;
        }
        
        _mapper.Map(parking, existingParking);
        
        existingParking.LastEditDate = isDayLightSaving ? DateTime.UtcNow.AddHours(1) : DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteParking(Guid id)
    {
        var parking = await _context.Parking.FirstOrDefaultAsync(p => p.Id == id);

        if (parking == null) return false;

        _context.Parking.Remove(parking);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ParkingHumanizedDto?> GetHumanizedParkingById(Guid id)
    {
        var parking = await _context.Parking.FirstOrDefaultAsync(p => p.Id == id);
        var parkingHumanized = new ParkingHumanizedDto();

        if (parking == null) return null;
        
        var verificationDateHumanized =
            parking.VerificationDate.ToOrdinalWords() + ", " + parking.VerificationDate.TimeOfDay;
        var lastEditDateHumanized = parking.LastEditDate.ToOrdinalWords() + ", " + parking.LastEditDate.TimeOfDay;
        var createdDateHumanized = parking.CreatedAt.ToOrdinalWords() + ", " + parking.CreatedAt.TimeOfDay;
        var pricingHumanized = parking.Price.ToString("C", new CultureInfo("en-GB"));

        _mapper.Map(parking, parkingHumanized);
        
        var info = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
        var isDayLightSaving = info.IsDaylightSavingTime(DateTime.UtcNow);
        
        parkingHumanized.HumanizedCreatedDate = createdDateHumanized;
        parkingHumanized.HumanizedLastEditDate = lastEditDateHumanized;
        parkingHumanized.HumanizedTimeLimit = parking.TimeLimit.Humanize();
        parkingHumanized.HumanizedDayLimit = parking.DayLimit.ToString();
        parkingHumanized.HumanizedPricing = pricingHumanized;
        parkingHumanized.HumanizedVerifiedDate = verificationDateHumanized;
        parkingHumanized.HumanizedLastEdit = isDayLightSaving ? 
            parking.LastEditDate.AddHours(-1).Humanize() : 
            parking.LastEditDate.Humanize();
        parkingHumanized.HumanizedCreatedAt = isDayLightSaving ? 
            parking.CreatedAt.AddHours(-1).Humanize() : 
            parking.CreatedAt.Humanize();
        
        return parkingHumanized;
    }
}