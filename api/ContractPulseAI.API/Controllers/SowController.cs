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
        private readonly IRfpService _rfpService;

        // Inject the dedicated SOW service we just built
        public SowController(ISowService sowService, IRfpService rfpService)
        {
            _sowService = sowService;
            _rfpService = rfpService;
        }

        /// <summary>
        /// Stateful RAG endpoint that generates a formal legal SOW document 
        /// using the original requirements, proposed RFP text, and master templates.
        /// </summary>
        [HttpPost("SOW_Generation")]
        public async Task<IActionResult> GenerateSowFromRfp(int id)
        {
            

            try
            {
                // Executes the full pipeline: Azure AI Search -> Azure OpenAI -> OpenXML docx -> SQL Update
                ClientRfpDto trackingResult = await _sowService.GenerateSowFromRfpAsync(id);

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

        /// <summary>
        /// NEW ADDITION: HTTP GET endpoint that compiles and streams the generated SOW document 
        /// straight out of memory as an instant browser file attachment download.
        /// </summary>
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadSowDocument(int id)
        {
            try
            {
                // 1. Fetch engagement record entries from your Azure SQL repository context via your service layer
                var rfpRecord = await _rfpService.GetRfpByIdAsync(id);

                if (rfpRecord == null)
                {
                    return NotFound($"SOW data records tracking index {id} could not be resolved.");
                }

                // 2. Call your high-performance OpenXML utility helper inside your service to parse paragraph strings
                // Make sure byte[] CreateOpenXmlWordDocument(string text) is defined inside your ISowService interface file!
                byte[] wordDocumentBytes = _sowService.CreateOpenXmlWordDocument(rfpRecord.RfpPrompt);

                string fileDownloadName = $"ContractPulse_SOW_{id}_{DateTime.UtcNow:yyyyMMdd}.docx";
                string contentMimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

                // 3. Streams the document file binary straight into the browser memory download stream tray
                return File(wordDocumentBytes, contentMimeType, fileDownloadName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred compiling file binaries stream: {ex.Message}");
            }
        }
    }
}
