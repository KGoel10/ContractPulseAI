using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractPulseAI.API.Models.Entities
{
    public class ResourcePricing
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("Position")]
        [StringLength(50)]
        public required string Position { get; set; }

        [Column("Allocation_Hours")]
        public required int AllocationHours { get; set; }

        [Column("Pricing_Per_Hour")]
        public required int PricingPerHour { get; set; }

        [Column("Created_Date")]
        public required DateTime CreatedDate { get; set; }

        [Column("LastUpdatedDate")]
        public DateTime? LastUpdatedDate { get; set; }
    }
}
