using FitSharp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public class InstructorReviewRepository : GenericRepository<InstructorReview>, IInstructorReviewRepository
    {
        private readonly DataContext _context;

        public InstructorReviewRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Instructor> GetInstructorWithReviewsAsync(int id)
        {
            return await _context.Instructors
                .Include(i => i.Reviews)
                .ThenInclude(r => r.Customer)
                .Where(i => i.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<InstructorReview>> GetAllReviewsWithRelatedDataByInstructorId(int id)
        {
            return await _context.InstructorReviews
                 .Include(r => r.Customer)
                 .ThenInclude(c => c.User)
                 .Include(r => r.Instructor)
                 .ThenInclude(i => i.User)
                 .Where(r => r.InstructorId == id)
                 .ToListAsync();
        }

        public async Task AddReviewAsync(InstructorReview review)
        {
            var instructor = await GetInstructorWithReviewsAsync(review.InstructorId);
            if (instructor == null)
            {
                return;
            }

            var customer = await _context.Customers.FindAsync(review.CustomerId);
            if (customer == null)
            {
                return;
            }

            review.Instructor = instructor;
            review.Customer = customer;

            await _context.InstructorReviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<InstructorReview>> GetReviewsByInstructorId(int instructorId)
        {
            var instructor = await GetInstructorWithReviewsAsync(instructorId);

            return instructor?.Reviews ?? Enumerable.Empty<InstructorReview>();
        }

        public async Task DeleteReview(InstructorReview review)
        {
            _context.InstructorReviews.Remove(review);
            await _context.SaveChangesAsync();
        }

        public async Task<InstructorReview> GetReviewWithAllRelatedDataByIdAsync(int id)
        {
            return await _context.InstructorReviews
                .Include(r => r.Customer)
                .ThenInclude(c => c.User)
                .Include(r => r.Instructor)
                .ThenInclude(i => i.User)
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<InstructorReview>> GetReviewsWithAllRelatedDataAsync()
        {
            return await _context.InstructorReviews
                .Include(r => r.Customer)
                .ThenInclude(c => c.User)
                .Include(r => r.Instructor)
                .ThenInclude(i => i.User)
                .ToListAsync();
        }

        public bool CustomerAlreadyReviewed(int customerId, int instructorId)
        {
            return _context.InstructorReviews.Any(r => r.CustomerId == customerId && r.InstructorId == instructorId);
        }
    }
}