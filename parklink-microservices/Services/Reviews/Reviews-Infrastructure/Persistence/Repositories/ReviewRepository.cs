using Microsoft.EntityFrameworkCore;
using Reviews_Domain.Data;
using Reviews_Domain.Entities;
using Reviews_Infrastructure.Persistence.Data;

namespace Reviews_Infrastructure.Persistence.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly ReviewDbContext _context;

    public ReviewRepository(ReviewDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Review>> GetReview()
    {
        var reviews = await _context.Review.AsNoTracking().ToListAsync();
        return reviews;
    }

    public async Task<Review?> GetReview(Guid id)
    {
        var review = await _context.Review.AsNoTracking().FirstOrDefaultAsync
            (p => p.Id == id);

        return review;
    }

    public async Task<Guid> CreateReview(Review review)
    {
        var generatedId = Guid.NewGuid();

        review.Id = generatedId;
        _context.Add(review);
        await _context.SaveChangesAsync();

        return generatedId;
    }

    public async Task<bool> UpdateReview(Review review)
    {
        var existingReview = await _context.Review.FirstOrDefaultAsync
            (p => p.Id == review.Id);

        if (existingReview == null) return false;

        existingReview.ReviewRating = review.ReviewRating;
        existingReview.ReviewTitle = review.ReviewTitle;
        existingReview.ReviewText = review.ReviewText;
        
        // adds the edit date automatically so you dont have to worry about it on the front end.
        existingReview.EditDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DeleteReview(Guid id)
    {
        var review = await _context.Review.FirstOrDefaultAsync
            (p => p.Id == id);
        
        if (review == null) return false;
        
        _context.Review.Remove(review);
        
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<ReviewAnalyticsDto> CheckRating(Guid parkingId)
    {
        var reviews = await _context.Review.AsNoTracking()
            .Where(p => p.ParkingId == parkingId).ToListAsync();

        if (!reviews.Any()) return new ReviewAnalyticsDto();
        
        var average = reviews.Average(r => r.ReviewRating);
        var reviewAnalytics = new ReviewAnalyticsDto
        {
            ParkingRating = average,
            Reviews = reviews
        };
        
        return reviewAnalytics;

    }
}