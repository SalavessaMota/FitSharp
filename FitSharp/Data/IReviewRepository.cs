using FitSharp.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<Instructor> GetInstructorWithReviewsAsync(int id);

        Task<IEnumerable<Review>> GetAllReviewsWithRelatedDataByInstructorId(int id);

        Task<IEnumerable<Review>> GetReviewsByInstructorId(int instructorId);

        Task AddReviewAsync(Review review);

        Task DeleteReview(Review review);

        Task<Review> GetReviewWithAllRelatedDataByIdAsync(int id);

        Task<IEnumerable<Review>> GetReviewsWithAllRelatedDataAsync();

        bool CustomerAlreadyReviewed(int customerId, int instructorId);
    }
}