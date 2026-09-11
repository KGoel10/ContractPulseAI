using ContractPulseAI.API.Data;
using ContractPulseAI.API.Models.Dtos;
using ContractPulseAI.API.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ContractPulseAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RfpController : ControllerBase
    {
        private readonly IRfpService _rfpService;

        public RfpController(IRfpService rfpService)
        {
            _rfpService = rfpService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRfps()
        {
            var rfps = await _rfpService.GetAllRfpsAsync();
            return Ok(rfps);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRfpById(int id)
        {
            var rfp = await _rfpService.GetRfpByIdAsync(id);

            if (rfp == null)
            {
                return NotFound();
            }

            return Ok(rfp);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRfp(ClientRfpDto dto)
        {
            var createdRfp = await _rfpService.CreateRfpAsync(dto);

            return CreatedAtAction(
                nameof(GetRfpById),
                new { id = createdRfp.Id },
                createdRfp);
        }
    }
}
