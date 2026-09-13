namespace ContractPulseAI.API.Models.Dtos
{
    public class RfpGenerationRequestDto
    {
        /// <summary>
        /// The raw requirement text context submitted by the user or client profile notes.
        /// </summary>
        public string ClientRequirement { get; set; } = string.Empty;

        /// <summary>
        /// Name of the client company (e.g., Acme Corp) to populate database records securely.
        /// </summary>
        public string? ClientName { get; set; }

        /// <summary>
        /// Contact email address associated with the specific deal workspace profile.
        /// </summary>
        public string? ClientEmail { get; set; }
    }
}
