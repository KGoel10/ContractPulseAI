using ContractPulseAI.API.Models.Entities;

namespace ContractPulseAI.API.Repositories.Interface
{
    public interface IRfpRepository
    {
        Task<List<ClientRFP>> GetAllAsync();

        Task<ClientRFP?> GetByIdAsync(int id);

        Task<ClientRFP> CreateAsync(ClientRFP rfp);

        /// <summary>
        /// Updates the execution state, generated file path links, 
        /// and timestamp parameters for an active client RFP entity tracking node.
        /// </summary>
        Task<ClientRFP> UpdateAsync(ClientRFP rfp);
    }
}
