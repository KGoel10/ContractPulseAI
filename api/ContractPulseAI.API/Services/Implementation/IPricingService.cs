using ContractPulseAI.API.Models.Dtos;

namespace ContractPulseAI.API.Services.Implementation
{
    public interface IPricingService
    {
        Task<List<ResourcePricingDto>> GetAllPricingsAsync();
        Task<ResourcePricingDto?> GetPricingByIdAsync(int id);
    }
}
