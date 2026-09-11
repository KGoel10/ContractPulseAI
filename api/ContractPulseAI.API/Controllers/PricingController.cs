using ContractPulseAI.API.Data;
using ContractPulseAI.API.Models.Dtos;
using ContractPulseAI.API.Services.Implementation;
using Microsoft.AspNetCore.Mvc;

namespace ContractPulseAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PricingController : ControllerBase
    {
        private readonly IPricingService _pricingService;
        public PricingController(IPricingService pricingService) 
        {
            _pricingService = pricingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPricings()
        {
            var pricings = await _pricingService.GetAllPricingsAsync();            
            return Ok(pricings);
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetPricingById(int id)
        {
            var pricing = await _pricingService.GetPricingByIdAsync(id);
            if (pricing == null)

            {
                return NotFound();
            }

            return Ok(pricing);
        }
    }
}
