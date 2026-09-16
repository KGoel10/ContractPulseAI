using Azure.Search.Documents; // Added for RAG capabilities
using Azure.Search.Documents.Models;
using ContractPulseAI.API.Models.Dtos;
using ContractPulseAI.API.Models.Entities;
using ContractPulseAI.API.Repositories.Interface;
using ContractPulseAI.API.Services.Interface;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenAI;
using OpenAI.Chat;
using System;
using System.ClientModel;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ContractPulseAI.API;

namespace ContractPulseAI.API.Services.Implementation
{
    public class SowService : ISowService
    {
        private readonly IRfpRepository _repository;
        private readonly ISowGenerationClient _sowClient; // Decoupled RAG heavy lifting client

        public SowService(IRfpRepository repository, ISowGenerationClient sowClient)
        {
            _repository = repository;
            _sowClient = sowClient;
        }

        public async Task<ClientRfpDto> GenerateSowFromRfpAsync(SowGenerationRequestDto request)
        {
            // STEP 1: SQL Data Intake (Fetch existing deal progress records)
            var rfpEntity = await _repository.GetByIdAsync(request.RfpId);
            if (rfpEntity == null)
            {
                throw new KeyNotFoundException($"RFP record with ID {request.RfpId} was not found.");
            }

            // STEP 2 & 3: Offload Manual RAG Search & Agent Execution entirely to your client wrapper
            // This strips out all static prompts and long string builders from your code!
            string generatedSowContent = await _sowClient.ExecuteRfpToSowPipelineAsync(
                rfpEntity.RFP_Prompt,
                request.Budget ?? "Not Specified",
                request.Duration ?? "Not Specified",
                request.Resource ?? "Not Specified", // Total number of headcount resources
                request.AdditionalPrompt
            );

            // STEP 4: In-Memory Stream URL Configuration (Bypasses Blob storage dependencies)
            // Sets a dynamic tracking route path right back to your local API download action
            rfpEntity.SOW_Link = $"/api/sow/download/{rfpEntity.ID}";
            rfpEntity.RFP_Status = "SOW_Generated";
            rfpEntity.LastUpdatedDate = DateTime.UtcNow;

            rfpEntity.RFP_Prompt = generatedSowContent;

            await _repository.UpdateAsync(rfpEntity);

            return new ClientRfpDto
            {
                Id = rfpEntity.ID,
                ClientName = rfpEntity.Client_Name,
                ClientEmail = rfpEntity.Client_Email,
                RfpPrompt = rfpEntity.RFP_Prompt,
                RfpLink = rfpEntity.RFP_Link,
                SowLink = rfpEntity.SOW_Link,
                RfpStatus = rfpEntity.RFP_Status,
                CreatedDate = rfpEntity.Created_Date,
                LastUpdatedDate = rfpEntity.LastUpdatedDate
            };
        }

        #region Private File Generation Helpers

        /// <summary>
        /// High-performance OpenXML template utility to compile word document byte streams on-the-fly.
        /// </summary>
        public byte[] CreateOpenXmlWordDocument(string textParagraphs)
        {
            using (MemoryStream memStream = new MemoryStream())
            {
                using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(memStream, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    Body body = mainPart.Document.AppendChild(new Body());

                    string[] lines = textParagraphs.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        Paragraph para = body.AppendChild(new Paragraph());
                        Run run = para.AppendChild(new Run());

                        // Simple dynamic support formatting for standard Markdown headers from LLM output arrays
                        if (line.StartsWith("###") || line.StartsWith("##") || line.StartsWith("#"))
                        {
                            string cleanHeader = line.Replace("#", "").Trim();
                            run.AppendChild(new Text(cleanHeader));

                            RunProperties runProps = new RunProperties();
                            runProps.AppendChild(new Bold());
                            runProps.AppendChild(new FontSize() { Val = "28" });
                            run.RunProperties = runProps;
                        }
                        else
                        {
                            run.AppendChild(new Text(line));
                        }
                    }

                    mainPart.Document.Save();
                }

                return memStream.ToArray();
            }
        }

        #endregion
    }
}
