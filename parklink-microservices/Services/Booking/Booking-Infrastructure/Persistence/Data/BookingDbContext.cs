using Booking_Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Booking_Infrastructure.Persistence.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Booking> Bookings { get; set; }
    public DbSet<BookingRecord> BookingRecords { get; set; }
    public DbSet<BookingRefund> BookingRefunds { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookingRefund>(entity =>
        {
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");
        });
        
        modelBuilder.Entity<Booking>(entity =>
        {
            // make sure that the default value is true when a new entity is inserted
            entity.Property(e => e.BookingConfirmation);
            entity.Property(e => e.Fees).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RefundStatus).HasDefaultValue(false);
            entity.Property(e => e.FinePaid).HasDefaultValue(false);
            entity.Property(e => e.FineStatus).HasDefaultValue(false);
            entity.Property(e => e.RefundAmount).HasColumnType("decimal(18, 2)").HasDefaultValue(0);
        });
        
        modelBuilder.Entity<BookingRecord>(entity =>
        {
            entity.Property(e => e.Fees).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.SubTotal).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DepositStatus).HasDefaultValue(false);
        });

        modelBuilder.Entity<Booking>()
            .HasKey(b => b.Id);

        modelBuilder.Entity<BookingRecord>()
            .HasKey(t => t.Id);
        
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.BookingRecord)
            .WithOne(t => t.Booking)
            .HasForeignKey<Booking>(b => b.RecordId);
    }
}