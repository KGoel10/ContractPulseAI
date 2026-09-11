using ContractPulseAI.API.Data;
using ContractPulseAI.API.Models.Entities;
using ContractPulseAI.API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace ContractPulseAI.API.Repositories.Implementation
{
    public class RfpRepository : IRfpRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public RfpRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ClientRFP>> GetAllAsync()
        {
            return await _dbContext.ClientRFPs.ToListAsync();
        }

        public async Task<ClientRFP?> GetByIdAsync(int id)
        {
            return await _dbContext.ClientRFPs
                .FirstOrDefaultAsync(r => r.ID == id);
        }

        public async Task<ClientRFP> CreateAsync(ClientRFP rfp)
        {
            _dbContext.ClientRFPs.Add(rfp);
            await _dbContext.SaveChangesAsync();

            return rfp;
        }
    }
}
