using ContractPulseAI.API.Models.Dtos;

namespace ContractPulseAI.API.Services.Interface
{
    public interface ISowService
    {
        /// <summary>
        /// Stateful RAG worker pipeline running manual vector lookups and gpt-5-mini threads.
        /// </summary>
        Task<ClientRfpDto> GenerateSowFromRfpAsync(int id);

        /// <summary>
        /// EXPOSES THE COMPILER: Connects the high-performance OpenXML utility to your controller.
        /// </summary>
        byte[] CreateOpenXmlWordDocument(string textParagraphs);
    }
}
