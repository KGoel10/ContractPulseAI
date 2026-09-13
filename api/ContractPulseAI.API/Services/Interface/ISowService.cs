using ContractPulseAI.API.Models.Dtos;

namespace ContractPulseAI.API.Services.Interface
{
    public interface ISowService
    {
        /// <summary>
        /// Combines existing RFP text, original requirements, and custom user prompts 
        /// to draft a formal Statement of Work using Azure OpenAI.
        /// </summary>
        Task<ClientRfpDto> GenerateSowFromRfpAsync(SowGenerationRequestDto request);
    }
}
