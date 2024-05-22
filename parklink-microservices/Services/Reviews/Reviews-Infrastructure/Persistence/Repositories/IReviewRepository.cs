using Reviews_Domain.Data;
using Reviews_Domain.Entities;

namespace Reviews_Infrastructure.Persistence.Repositories;

public interface IReviewRepository
{
    public Task<List<Review>> GetReview();
    public Task<Review?> GetReview(Guid id);
    public Task<Guid> CreateReview(Review review);
    public Task<bool> UpdateReview(Review review);
    public Task<bool> DeleteReview(Guid id);
    public Task<ReviewAnalyticsDto> CheckRating(Guid parkingId);
}