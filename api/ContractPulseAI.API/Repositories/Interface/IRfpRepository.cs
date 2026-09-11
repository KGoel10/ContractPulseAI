using ContractPulseAI.API.Models.Entities;

namespace ContractPulseAI.API.Repositories.Interface
{
    public interface IRfpRepository
    {
        Task<List<ClientRFP>> GetAllAsync();

        Task<ClientRFP?> GetByIdAsync(int id);

        Task<ClientRFP> CreateAsync(ClientRFP rfp);
    }
}
