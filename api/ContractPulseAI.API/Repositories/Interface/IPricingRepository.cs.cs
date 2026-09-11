using ContractPulseAI.API.Models.Entities;

namespace ContractPulseAI.API.Repositories.Interface
{
    public interface IPricingRepository
    {
        Task<List<ResourcePricing>> GetAllAsync();
        Task<ResourcePricing?> GetByIdAsync(int id);
    }
}
