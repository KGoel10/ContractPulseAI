using ContractPulseAI.API.Models.Dtos;

namespace ContractPulseAI.API.Services.Interface
{
    public interface IRfpService
    {
        Task<List<ClientRfpDto>> GetAllRfpsAsync();

        Task<ClientRfpDto?> GetRfpByIdAsync(int id);

        Task<ClientRfpDto> CreateRfpAsync(ClientRfpDto dto);
    }
}
