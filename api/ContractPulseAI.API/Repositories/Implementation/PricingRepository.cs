using ContractPulseAI.API.Data;
using ContractPulseAI.API.Models.Entities;
using ContractPulseAI.API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace ContractPulseAI.API.Repositories.Implementation
{
    public class PricingRepository : IPricingRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PricingRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ResourcePricing>> GetAllAsync()
        {
            return await _dbContext.ResourcePricings.ToListAsync();
        }

        public async Task<ResourcePricing?> GetByIdAsync(int id)
        {
            return await _dbContext.ResourcePricings
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
