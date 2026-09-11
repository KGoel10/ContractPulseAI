using System.ComponentModel.DataAnnotations;

namespace ContractPulseAI.API.Models.Entities
{
    public class ClientRFP
    {
        [Key]
        public int ID { get; set; }

        [StringLength(255)]
        public required string Client_Name { get; set; }

        [StringLength(150)]
        public required string Client_Email { get; set; }

        [StringLength(5000)]
        public string? RFP_Prompt { get; set; }

        [StringLength(255)]
        public string? RFP_Link { get; set; }

        [StringLength(255)]
        public string? SOW_Link { get; set; }

        [StringLength(100)]
        public string? RFP_Status { get; set; }

        public required DateTime Created_Date { get; set; }

        public DateTime? LastUpdatedDate { get; set; }
    }
}
