using Microsoft.EntityFrameworkCore;

namespace ContractPulseAI.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Models.Entities.ResourcePricing> ResourcePricings { get; set; }
        public DbSet<Models.Entities.ClientRFP> ClientRFPs { get; set; }
    }
}
