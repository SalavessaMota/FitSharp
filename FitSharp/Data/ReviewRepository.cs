using FitSharp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        private readonly DataContext _context;

        public ReviewRepository(DataContext context) : base(context)
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

        public async Task<IEnumerable<Review>> GetAllReviewsWithRelatedDataByInstructorId(int id)
        {
            return await _context.Reviews
                 .Include(r => r.Customer)
                 .ThenInclude(c => c.User)
                 .Include(r => r.Instructor)
                 .ThenInclude(i => i.User)
                 .Where(r => r.InstructorId == id)
                 .ToListAsync();
        }

        public async Task AddReviewAsync(Review review)
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

            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsByInstructorId(int instructorId)
        {
            var instructor = await GetInstructorWithReviewsAsync(instructorId);

            return instructor?.Reviews ?? Enumerable.Empty<Review>();
        }

        public async Task DeleteReview(Review review)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
        }

        public async Task<Review> GetReviewWithAllRelatedDataByIdAsync(int id)
        {
            return await _context.Reviews
                .Include(r => r.Customer)
                .ThenInclude(c => c.User)
                .Include(r => r.Instructor)
                .ThenInclude(i => i.User)
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Review>> GetReviewsWithAllRelatedDataAsync()
        {
            return await _context.Reviews
                .Include(r => r.Customer)
                .ThenInclude(c => c.User)
                .Include(r => r.Instructor)
                .ThenInclude(i => i.User)
                .ToListAsync();
        }

        public bool CustomerAlreadyReviewed(int customerId, int instructorId)
        {
            return _context.Reviews.Any(r => r.CustomerId == customerId && r.InstructorId == instructorId);
        }
    }
}