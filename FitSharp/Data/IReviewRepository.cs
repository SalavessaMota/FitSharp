using FitSharp.Data.Entities;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<Instructor> GetInstructorWithReviewsAsync(int id);

        Task<IEnumerable<Review>> GetReviewsByInstructorId(int instructorId);

        Task AddReviewAsync(Review review);

        Task DeleteReview(Review review);

        Task<IEnumerable<Review>> GetReviewsWithAllRelatedDataAsync();
    }
}
