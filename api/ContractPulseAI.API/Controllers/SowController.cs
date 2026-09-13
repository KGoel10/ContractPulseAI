using ContractPulseAI.API.Models.Dtos;
using ContractPulseAI.API.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ContractPulseAI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SowController : ControllerBase
    {
        private readonly ISowService _sowService;

        // Inject the dedicated SOW service we just built
        public SowController(ISowService sowService)
        {
            _sowService = sowService;
        }

        /// <summary>
        /// Stateful RAG endpoint that generates a formal legal SOW document 
        /// using the original requirements, proposed RFP text, and master templates.
        /// </summary>
        [HttpPost("SOW_Generation")]
        public async Task<IActionResult> GenerateSowFromRfp([FromBody] SowGenerationRequestDto request)
        {
            if (request == null || request.RfpId <= 0)
            {
                return BadRequest("A valid RFP ID must be provided.");
            }

            try
            {
                // Executes the full pipeline: Azure AI Search -> Azure OpenAI -> OpenXML docx -> SQL Update
                ClientRfpDto trackingResult = await _sowService.GenerateSowFromRfpAsync(request);

                return Ok(trackingResult);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // Log the exception using your preferred logging setup
                return StatusCode(500, $"An error occurred during SOW generation: {ex.Message}");
            }
        }
    }
}
