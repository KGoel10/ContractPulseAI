namespace ContractPulseAI.API.Services.Interface
{
    public interface ISowGenerationClient
    {
        Task<string> ExecuteRfpToSowPipelineAsync(string rfpPrompt, string budget, string duration, string resource, string? additionalPrompt);
    }
}
