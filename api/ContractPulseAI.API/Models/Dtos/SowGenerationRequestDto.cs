namespace ContractPulseAI.API.Models.Dtos
{
    public class SowGenerationRequestDto
    {
        /// <summary>
        /// The unique database ID of the active client engagement record.
        /// </summary>
        public int RfpId { get; set; }

        /// <summary>
        /// Optional instructions, constraints, or custom prompts from the user to guide the SOW generation.
        /// </summary>
        public string? AdditionalPrompt { get; set; }
    }
}
