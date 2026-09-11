namespace ContractPulseAI.API.Models.Dtos
{
    public class ClientRfpDto
    {
        public int Id { get; set; }

        public required string ClientName { get; set; }

        public required string ClientEmail { get; set; }

        public string? RfpPrompt { get; set; }

        public string? RfpLink { get; set; }

        public string? SowLink { get; set; }

        public string? RfpStatus { get; set; }

        public required DateTime CreatedDate { get; set; }

        public DateTime? LastUpdatedDate { get; set; }
    }
}
