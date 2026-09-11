namespace ContractPulseAI.API.Models.Dtos
{
    using System;

    public class ResourcePricingDto
    {
        public int Id { get; set; }

        public required string Position { get; set; }

        public required int AllocationHours { get; set; }

        public required int PricingPerHour { get; set; }

        public required DateTime CreatedDate { get; set; }
        public DateTime? LastUpdatedDate { get; set; }
    }
}
