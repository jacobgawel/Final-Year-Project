using Microsoft.EntityFrameworkCore;
using Parking_Domain.Entities;

namespace Parking_Infrastructure.Data;

public class ParkingDbContext : DbContext
{
    public ParkingDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Parking> Parking { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Parking>(entity =>
        {
            entity.Property(e => e.VerificationStatus).HasDefaultValue(false);
            entity.Property(e => e.AvailabilityStatus).HasDefaultValue(false);
            entity.Property(e => e.ParkingRejected).HasDefaultValue(false);
            entity.Property(e => e.Price).HasColumnType("decimal(18, 2)");
        });
    }
}