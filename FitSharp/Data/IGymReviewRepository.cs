using FitSharp.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public interface IGymReviewRepository : IGenericRepository<GymReview>
    {
        Task<Gym> GetGymWithReviewsAsync(int id);

        Task<IEnumerable<GymReview>> GetAllReviewsWithRelatedDataByGymId(int id);

        Task<IEnumerable<GymReview>> GetReviewsByGymId(int gymId);

        Task AddReviewAsync(GymReview review);

        Task DeleteReview(GymReview review);

        Task<GymReview> GetReviewWithAllRelatedDataByIdAsync(int id);

        Task<IEnumerable<GymReview>> GetReviewsWithAllRelatedDataAsync();

        bool CustomerAlreadyReviewed(int customerId, int gymId);
    }
}