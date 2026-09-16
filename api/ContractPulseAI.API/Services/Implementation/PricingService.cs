using ContractPulseAI.API.Models.Dtos;
using ContractPulseAI.API.Repositories.Interface;
using ContractPulseAI.API.Services.Interface;

namespace ContractPulseAI.API.Services.Implementation
{
    public class PricingService : IPricingService
    {
        private readonly IPricingRepository _repository;

        public PricingService(IPricingRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ResourcePricingDto>> GetAllPricingsAsync()
        {
            var pricings = await _repository.GetAllAsync();

            return pricings.Select(pricing => new ResourcePricingDto
            {
                Id = pricing.Id,
                AllocationHours = pricing.AllocationHours,
                Position = pricing.Position,
                PricingPerHour = pricing.PricingPerHour,
                CreatedDate = pricing.CreatedDate,
                LastUpdatedDate = pricing.LastUpdatedDate
            }).ToList();
        }

        public async Task<ResourcePricingDto?> GetPricingByIdAsync(int id)
        {
            var pricing = await _repository.GetByIdAsync(id);

            if (pricing == null)
                return null;

            return new ResourcePricingDto
            {
                Id = pricing.Id,
                AllocationHours = pricing.AllocationHours,
                Position = pricing.Position,
                PricingPerHour = pricing.PricingPerHour,
                CreatedDate = pricing.CreatedDate,
                LastUpdatedDate = pricing.LastUpdatedDate
            };
        }
    }
}
