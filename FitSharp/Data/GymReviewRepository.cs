using FitSharp.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Data
{
    public class GymReviewRepository : GenericRepository<GymReview>, IGymReviewRepository
    {
        private readonly DataContext _context;

        public GymReviewRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Gym> GetGymWithReviewsAsync(int id)
        {
            return await _context.Gyms
                .Include(i => i.Reviews)
                .ThenInclude(r => r.Customer)
                .Where(i => i.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<GymReview>> GetAllReviewsWithRelatedDataByGymId(int id)
        {
            return await _context.GymReviews
                 .Include(r => r.Customer)
                 .ThenInclude(c => c.User)
                 .Include(r => r.Gym)
                 .Where(r => r.GymId == id)
                 .ToListAsync();
        }

        public async Task AddReviewAsync(GymReview review)
        {
            var gym = await GetGymWithReviewsAsync(review.GymId);
            if (gym == null)
            {
                return;
            }

            var customer = await _context.Customers.FindAsync(review.CustomerId);
            if (customer == null)
            {
                return;
            }

            review.Gym = gym;
            review.Customer = customer;

            await _context.GymReviews.AddAsync(review);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<GymReview>> GetReviewsByGymId(int gymId)
        {
            var gym = await GetGymWithReviewsAsync(gymId);

            return gym?.Reviews ?? Enumerable.Empty<GymReview>();
        }

        public async Task DeleteReview(GymReview review)
        {
            _context.GymReviews.Remove(review);
            await _context.SaveChangesAsync();
        }

        public async Task<GymReview> GetReviewWithAllRelatedDataByIdAsync(int id)
        {
            return await _context.GymReviews
                .Include(r => r.Customer)
                .ThenInclude(c => c.User)
                .Include(r => r.Gym)
                .Where(r => r.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<GymReview>> GetReviewsWithAllRelatedDataAsync()
        {
            return await _context.GymReviews
                .Include(r => r.Customer)
                .ThenInclude(c => c.User)
                .Include(r => r.Gym)
                .ToListAsync();
        }

        public bool CustomerAlreadyReviewed(int customerId, int gymId)
        {
            return _context.GymReviews.Any(r => r.CustomerId == customerId && r.GymId == gymId);
        }
    }
}