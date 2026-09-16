using ContractPulseAI.API.Models.Dtos;

namespace ContractPulseAI.API.Services.Interface
{
    public interface IRfpService
    {
        Task<List<ClientRfpDto>> GetAllRfpsAsync();

        Task<ClientRfpDto?> GetRfpByIdAsync(int id);

        Task<ClientRfpDto> CreateRfpAsync(ClientRfpDto dto);

        Task<ClientRfpDto> UpdateRfpAsync(ClientRfpDto dto);

        // Updated to process complex request objects and return full stateful records
        Task<ClientRfpDto> GenerateRfpWordDocumentAsync(RfpGenerationRequestDto request);
    }
}
