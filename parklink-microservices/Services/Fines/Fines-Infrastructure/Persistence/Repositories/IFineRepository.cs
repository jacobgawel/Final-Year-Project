using Fines_Domain.Data;
using Fines_Domain.Entities;

namespace Fines_Infrastructure.Persistence.Repositories;

public interface IFineRepository
{
    Task<Guid> CreateFine(Fine fine);
    Task<List<Fine>> GetFine();
    Task<List<Fine>> GetFinesForProvider(Guid providerId);
    Task<Fine?> GetFine(Guid id);
    Task<bool> UpdateFine(FineUpdateDto fine);
    Task<bool> DeleteFine(Guid id);
    Task<List<Fine>> GetFineByAccountId(Guid id);
    Task<bool> CheckIfBookingHasFine(Guid id);
    Task<FineAnalyticsDto> GetFinesAnalyticsForProvider(Guid providerId);
    Task<FineAnalyticsDto> GetFinesAnalyticsForAdmin();
}