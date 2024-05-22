using Fines_Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fines_Infrastructure.Persistence.Data;

public class FineDbContext : DbContext
{
    public FineDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fine>(entity =>
        {
            entity.Property(e => e.Total).HasColumnType("decimal(18, 2)");
        });
    }

    public DbSet<Fine> Fines { get; set; }
}