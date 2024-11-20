using FitSharp.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public interface IInstructorReviewRepository : IGenericRepository<InstructorReview>
    {
        Task<Instructor> GetInstructorWithReviewsAsync(int id);

        Task<IEnumerable<InstructorReview>> GetAllReviewsWithRelatedDataByInstructorId(int id);

        Task<IEnumerable<InstructorReview>> GetReviewsByInstructorId(int instructorId);

        Task AddReviewAsync(InstructorReview review);

        Task DeleteReview(InstructorReview review);

        Task<InstructorReview> GetReviewWithAllRelatedDataByIdAsync(int id);

        Task<IEnumerable<InstructorReview>> GetReviewsWithAllRelatedDataAsync();

        bool CustomerAlreadyReviewed(int customerId, int instructorId);
    }
}