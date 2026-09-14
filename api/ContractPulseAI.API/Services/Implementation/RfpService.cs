using System.IO;
using System.Text;
using System.ClientModel; // Required for ApiKeyCredential / AzureOpenAIClient config in newer OpenAI SDK specs
using ContractPulseAI.API.Models.Dtos;
using ContractPulseAI.API.Models.Entities;
using ContractPulseAI.API.Repositories.Interface;
using ContractPulseAI.API.Services.Interface;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenAI;
using OpenAI.Chat;

namespace ContractPulseAI.API.Services.Implementation
{
    public class RfpService : IRfpService
    {
        private readonly IRfpRepository _repository;
        private readonly ChatClient _chatClient;

        public RfpService(IRfpRepository repository)
        {
            _repository = repository;

            // Fetch Azure configuration settings from system environment keys or fallback defaults
            string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? "https://azure.com";
            string apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? "your-api-key";
            string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4o";

            // Initialize the OpenAI client using the official Azure SDK standard pattern
            // Note: If using Azure Open AI infrastructure, you can initialize via AzureOpenAIClient
            var azureClient = new global::Azure.AI.OpenAI.AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
            _chatClient = azureClient.GetChatClient(deploymentName);
        }

        public async Task<List<ClientRfpDto>> GetAllRfpsAsync()
        {
            var rfps = await _repository.GetAllAsync();

            return rfps.Select(rfp => new ClientRfpDto
            {
                Id = rfp.ID,
                ClientName = rfp.Client_Name,
                ClientEmail = rfp.Client_Email,
                RfpPrompt = rfp.RFP_Prompt,
                RfpLink = rfp.RFP_Link,
                SowLink = rfp.SOW_Link,
                RfpStatus = rfp.RFP_Status,
                CreatedDate = rfp.Created_Date,
                LastUpdatedDate = rfp.LastUpdatedDate
            }).ToList();
        }

        public async Task<ClientRfpDto?> GetRfpByIdAsync(int id)
        {
            var rfp = await _repository.GetByIdAsync(id);

            if (rfp == null)
                return null;

            return new ClientRfpDto
            {
                Id = rfp.ID,
                ClientName = rfp.Client_Name,
                ClientEmail = rfp.Client_Email,
                RfpPrompt = rfp.RFP_Prompt,
                RfpLink = rfp.RFP_Link,
                SowLink = rfp.SOW_Link,
                RfpStatus = rfp.RFP_Status,
                CreatedDate = rfp.Created_Date,
                LastUpdatedDate = rfp.LastUpdatedDate
            };
        }

        public async Task<ClientRfpDto> CreateRfpAsync(ClientRfpDto dto)
        {
            var entity = new ClientRFP
            {
                Client_Name = dto.ClientName,
                Client_Email = dto.ClientEmail,
                RFP_Prompt = dto.RfpPrompt,
                RFP_Link = dto.RfpLink,
                SOW_Link = dto.SowLink,
                RFP_Status = dto.RfpStatus,
                Created_Date = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow
            };

            entity = await _repository.CreateAsync(entity);

            return new ClientRfpDto
            {
                Id = entity.ID,
                ClientName = entity.Client_Name,
                ClientEmail = entity.Client_Email,
                RfpPrompt = entity.RFP_Prompt,
                RfpLink = entity.RFP_Link,
                SowLink = entity.SOW_Link,
                RfpStatus = entity.RFP_Status,
                CreatedDate = entity.Created_Date,
                LastUpdatedDate = entity.LastUpdatedDate
            };
        }

        public async Task<ClientRfpDto> GenerateRfpWordDocumentAsync(RfpGenerationRequestDto request)
        {
            // 1. WHITEBOARD STEP: SQL -> RFP (Initial Entry)
            var rfpEntity = new ClientRFP
            {
                Client_Name = request.ClientName ?? "Unknown Client",
                Client_Email = request.ClientEmail ?? "unknown@client.com",
                RFP_Prompt = request.ClientRequirement,
                RFP_Status = "Processing",
                Created_Date = DateTime.UtcNow,
                LastUpdatedDate = DateTime.UtcNow
            };

            rfpEntity = await _repository.CreateAsync(rfpEntity);

            // 2. WHITEBOARD STEP: PII Agent Call
            var (sanitizedRequirement, tokenDictionary) = ApplySanitizationGateway(rfpEntity.RFP_Prompt);

            // 3. WHITEBOARD STEP: RFP Generation via LLM using pre-configured _chatClient
            List<ChatMessage> messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an expert IT staff-augmentation RFP writer. Generate a comprehensive, professional technical capability response based on the client requirements."),
                new UserChatMessage(sanitizedRequirement)
            };

            ChatCompletion completion = await _chatClient.CompleteChatAsync(messages);
            string generatedContentPlaceholder = completion.Content[0].Text;

            // 4. DE-ANONYMIZATION
            string finalizedTextContent = RehydrateTokens(generatedContentPlaceholder, tokenDictionary);

            // 5. WHITEBOARD STEP: File Generation (OpenXML)
            byte[] docxBytes = CreateOpenXmlWordDocument(finalizedTextContent);

            // 6. STATE UPDATE: Save generated links and update status
            rfpEntity.RFP_Link = $"https://windows.net_{rfpEntity.ID}.docx";
            rfpEntity.RFP_Status = "Generated";
            rfpEntity.LastUpdatedDate = DateTime.UtcNow;

            rfpEntity.RFP_Prompt = finalizedTextContent; // Optionally store the generated content for reference

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

        #region Private Architecture Helpers

        private (string SanitizedText, Dictionary<string, string> TokenMap) ApplySanitizationGateway(string rawText)
        {
            var tokenMap = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(rawText)) return (rawText, tokenMap);

            string sanitized = rawText;

            // Quick demo check of custom PII scrubbing patterns matching client variables
            if (sanitized.Contains("Acme Corp"))
            {
                tokenMap.Add("[CLIENT_A]", "Acme Corp");
                sanitized = sanitized.Replace("Acme Corp", "[CLIENT_A]");
            }

            return (sanitized, tokenMap);
        }

        private string RehydrateTokens(string text, Dictionary<string, string> tokenMap)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string rehydrated = text;
            foreach (var kvp in tokenMap)
            {
                rehydrated = rehydrated.Replace(kvp.Key, kvp.Value);
            }
            return rehydrated;
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

                        // Simple support formatting for standard Markdown headers from LLM output arrays
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
