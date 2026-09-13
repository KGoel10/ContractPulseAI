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

namespace ContractPulseAI.API.Services.Implementation
{
    public class SowService : ISowService
    {
        private readonly IRfpRepository _repository;
        private readonly ChatClient _chatClient;
        private readonly SearchClient _searchClient;

        public SowService(IRfpRepository repository)
        {
            _repository = repository;

            // 1. Fetch Environment/Configuration Strings
            string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "https://azure.com";
            string apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? "your-api-key";
            string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4o";

            string searchEndpoint = Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT") ?? "https://windows.net";
            string searchApiKey = Environment.GetEnvironmentVariable("AZURE_SEARCH_API_KEY") ?? "your-search-key";
            string searchIndexName = Environment.GetEnvironmentVariable("AZURE_SEARCH_INDEX_SOW") ?? "sow-legal-templates";

            // 2. Initialize Azure Connections
            var azureClient = new global::Azure.AI.OpenAI.AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
            _chatClient = azureClient.GetChatClient(deploymentName);

            // Dedicated search client to query master SOPs and compliance templates
            _searchClient = new SearchClient(new Uri(searchEndpoint), searchIndexName, new global::Azure.AzureKeyCredential(searchApiKey));
        }

        public async Task<ClientRfpDto> GenerateSowFromRfpAsync(SowGenerationRequestDto request)
        {
            // STEP 1: SQL Data Intake (Fetch existing deal progress records)
            var rfpEntity = await _repository.GetByIdAsync(request.RfpId);
            if (rfpEntity == null)
            {
                throw new KeyNotFoundException($"RFP record with ID {request.RfpId} was not found.");
            }

            // STEP 2: Azure AI Search Execution (The RAG Step)
            // We search your corporate knowledge base using the core client requirements
            string retrievedLegalContext = await RetrieveMasterContractTemplatesAsync(rfpEntity.RFP_Prompt);

            // STEP 3: Build the Augmented System Directive String
            var systemContextBuilder = new StringBuilder();
            systemContextBuilder.AppendLine("You are an expert commercial contract writer for an IT staff-augmentation consulting firm.");
            systemContextBuilder.AppendLine("Your job is to draft a formal legal Statement of Work (SOW) by combining the initial client requirements, our proposed solution text, and our company's standard master legal templates.");
            systemContextBuilder.AppendLine("The SOW must include explicit sections for: 1. Scope Boundaries, 2. Service Level Agreements (SLAs), 3. Onboarding Terms, and 4. Resource Billing Schedules. Use clear markdown headings.");
            systemContextBuilder.AppendLine();
            systemContextBuilder.AppendLine("### MANDATORY LEGAL TEMPLATES & STANDARD CLAUSES ###");
            systemContextBuilder.AppendLine("You must follow these standard company clauses and structural layouts retrieved from our database:");
            systemContextBuilder.AppendLine(retrievedLegalContext); // Injecting the RAG content

            // STEP 4: Combine Deal Specific In-Flight Data Sources
            var userPromptBuilder = new StringBuilder();
            userPromptBuilder.AppendLine("### INITIAL CLIENT REQUIREMENT ###");
            userPromptBuilder.AppendLine(rfpEntity.RFP_Prompt);
            userPromptBuilder.AppendLine();
            userPromptBuilder.AppendLine("### OUR PROPOSED RFP TECHNICAL RESPONSE FILE REFERENCE ###");
            userPromptBuilder.AppendLine(rfpEntity.RFP_Link);

            if (!string.IsNullOrWhiteSpace(request.AdditionalPrompt))
            {
                userPromptBuilder.AppendLine();
                userPromptBuilder.AppendLine("### USER REVISION PROMPTS & SPECIFIC INSTRUCTIONS ###");
                userPromptBuilder.AppendLine(request.AdditionalPrompt);
            }

            // STEP 5: Trigger Azure OpenAI Reasoning Evaluation
            List<ChatMessage> messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemContextBuilder.ToString()),
                new UserChatMessage(userPromptBuilder.ToString())
            };

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);
            string generatedSowContent = completion.Content[0].Text;

            // STEP 6: High-Performance OpenXML File Compilation
            byte[] docxBytes = CreateOpenXmlWordDocument(generatedSowContent);

            // STEP 7: State Machine Status Tracking Persistence
            rfpEntity.SOW_Link = $"https://windows.net_{rfpEntity.ID}.docx";
            rfpEntity.RFP_Status = "SOW_Generated";
            rfpEntity.LastUpdatedDate = DateTime.UtcNow;

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

        #region Private RAG & File Generation Helpers

        private async Task<string> RetrieveMasterContractTemplatesAsync(string searchInputQuery)
        {
            var searchBuilder = new StringBuilder();

            try
            {
                // Configures keywords or intent boundaries to fetch top compliance document blocks
                var searchOptions = new SearchOptions
                {
                    Size = 3, // Pull the top 3 most relevant matches from the index
                    Select = { "content", "metadata_title" } // Matches the field structure in Azure AI Search
                };

                var searchResultResponse = await _searchClient.SearchAsync<SearchDocument>(searchInputQuery, searchOptions);

                await foreach (var result in searchResultResponse.Value.GetResultsAsync())
                {
                    if (result.Document.TryGetValue("content", out var contentText))
                    {
                        searchBuilder.AppendLine($"--- Standard Clause Reference ---");
                        searchBuilder.AppendLine(contentText?.ToString());
                    }
                }
            }
            catch (Exception)
            {
                // Fallback default in case the search server environment properties aren't wired up yet
                searchBuilder.AppendLine("Standard Clause: Standard 30-day payment windows apply. Replacement window for vacancies defaults to 10 business days.");
            }

            return searchBuilder.Length > 0 ? searchBuilder.ToString() : "No company master clauses found matching the request parameters.";
        }

        private byte[] CreateOpenXmlWordDocument(string textParagraphs)
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
