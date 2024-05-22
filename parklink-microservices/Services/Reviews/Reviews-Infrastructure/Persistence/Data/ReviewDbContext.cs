using Microsoft.EntityFrameworkCore;
using Reviews_Domain.Entities;

namespace Reviews_Infrastructure.Persistence.Data;

public class ReviewDbContext : DbContext
{
    public ReviewDbContext(DbContextOptions<ReviewDbContext> options) : base(options)
    {
    }

    public DbSet<Review> Review { get; set; }
}