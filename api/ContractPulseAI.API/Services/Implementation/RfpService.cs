using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using ContractPulseAI.API.Models.Dtos;
using ContractPulseAI.API.Models.Entities;
using ContractPulseAI.API.Repositories.Interface;
using ContractPulseAI.API.Services.Interface;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using OpenAI.Chat;

namespace ContractPulseAI.API.Services.Implementation
{
    public class RfpService : IRfpService
    {
        private readonly IRfpRepository _repository;
        private readonly AgentsClient _agentsClient;
        private readonly string _agentId;

        public RfpService(IRfpRepository repository, IConfiguration configuration, AgentsClient agentsClient)
        {
            _repository = repository;

            //// 1. Pull the unified connection string from appsettings.json
            //string connectionString = configuration["AzureFoundrySettings:ProjectConnectionString"]
            //    ?? throw new InvalidOperationException("Azure AI Foundry Project Connection String is missing from configurations.");

            //// 2. Pull the deployed gpt-5-mini Agent ID from appsettings.json
            //_agentId = configuration["AzureFoundrySettings:AgentId"]
            //    ?? throw new InvalidOperationException("Azure AI Agent ID is missing from configurations.");

            //// 3. CLEAN BYPASS WORKAROUND: Extract project key and supply it via our custom provider wrapper
            //string foundryApiKey = configuration["AzureFoundrySettings:ApiKey"] ?? "YOUR_FOUNDRY_PROJECT_API_KEY";

            //// Fulfills the exact TokenCredential argument without assembly conflicts!
            //var cleanCredential = new CustomTokenCredentialProvider(foundryApiKey);

            //_agentsClient = new AgentsClient(connectionString, cleanCredential);

            _agentsClient = agentsClient;
            _agentId = configuration["AzureFoundrySettings:AgentId"]
                ?? throw new InvalidOperationException("Azure AI Agent ID is missing from configurations.");

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

        public async Task<ClientRfpDto> UpdateRfpAsync(ClientRfpDto dto)
        {
            var rfp = _repository.GetByIdAsync(dto.Id).Result;
            if(rfp is not null)
            {
                rfp.RFP_Prompt = dto.RfpPrompt;
                rfp.RFP_Link = dto.RfpLink;
                rfp.SOW_Link = dto.SowLink;
                rfp.RFP_Status = dto.RfpStatus;
                rfp.LastUpdatedDate = DateTime.UtcNow;
                var entity = await _repository.UpdateAsync(rfp);

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
            else
            {
                throw new Exception($"RFP with ID {dto.Id} not found.");
            }
        }

        public async Task<ClientRfpDto> GenerateRfpWordDocumentAsync(int id)
        {
            var rfpEntity = _repository.GetByIdAsync(id).Result;

            if(rfpEntity is null)
            {
                throw new Exception($"RFP with ID {id} not found.");
            }

            // 1. WHITEBOARD STEP: SQL -> RFP (Initial Entry)
            rfpEntity.RFP_Status = "Processing";
            rfpEntity.LastUpdatedDate = DateTime.UtcNow;

            rfpEntity = await _repository.UpdateAsync(rfpEntity);

            // 2. WHITEBOARD STEP: PII Agent Call
            var (sanitizedRequirement, tokenDictionary) = ApplySanitizationGateway(rfpEntity.RFP_Prompt, rfpEntity.Client_Name, rfpEntity.Client_Email);

            string generatedContentPlaceholder = string.Empty;


            try
            {
                // 3. WHITEBOARD STEP: Foundry Agent Conversation Pipeline (Replaces ChatClient)
                // A. Provision an isolated conversation container thread on Azure AI Foundry
                Response<AgentThread> threadResponse = await _agentsClient.CreateThreadAsync();
                string threadId = threadResponse.Value.Id;

                // B. Push the sanitized prompt text as a User Message onto the active thread
                Response<ThreadMessage> messageResponse = await _agentsClient.CreateMessageAsync(
                    threadId,
                    MessageRole.User,
                    sanitizedRequirement
                );

                // C. Fire up the Agent execution run using your configured gpt-5-mini Agent ID
                Response<ThreadRun> runResponse = await _agentsClient.CreateRunAsync(threadId, _agentId);
                string runId = runResponse.Value.Id;

                // D. Polling Loop: Check execution states until the model finishes building content sections
                ThreadRun currentRun;
                do
                {
                    await Task.Delay(TimeSpan.FromSeconds(1)); // Polls every 1 second for rapid working
                    Response<ThreadRun> checkResponse = await _agentsClient.GetRunAsync(threadId, runId);
                    currentRun = checkResponse.Value;
                }

                while (currentRun.Status.ToString().Equals("queued", StringComparison.OrdinalIgnoreCase) ||
           currentRun.Status.ToString().Equals("in_progress", StringComparison.OrdinalIgnoreCase) ||
           currentRun.Status.ToString().Equals("inprogress", StringComparison.OrdinalIgnoreCase));

                if (currentRun.Status.ToString().Equals("completed", StringComparison.OrdinalIgnoreCase))
                {
                    Response<PageableList<ThreadMessage>> listResponse = await _agentsClient.GetMessagesAsync(threadId);
                    var assistantMessage = listResponse.Value.Data.FirstOrDefault(m =>
                        m.Role.ToString().Equals("agent", StringComparison.OrdinalIgnoreCase) ||
                        m.Role.ToString().Equals("assistant", StringComparison.OrdinalIgnoreCase));

                    if (assistantMessage != null)
                    {
                        foreach (var contentItem in assistantMessage.ContentItems)
                        {
                            if (contentItem is MessageTextContent textItem)
                            {
                                generatedContentPlaceholder = textItem.Text;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception($"Azure AI Foundry Agent run failed with fatal cloud runtime status: {currentRun.Status}");
                }
            }
            catch (Exception ex)
            {
                // Update database log state to track runtime exceptions clearly
                rfpEntity.RFP_Status = "Failed";
                await _repository.UpdateAsync(rfpEntity);
                throw new Exception($"Failed during Azure AI Agent cloud execution pipeline. Details: {ex.Message}", ex);
            }

            // 4. DE-ANONYMIZATION
            string finalizedTextContent = RehydrateTokens(generatedContentPlaceholder, tokenDictionary);

            // 5. MEMORY-ONLY STREAM CONFIGURATION (Bypasses Blob Storage completely)
            // Generate the path pointer straight back to your local API stream action endpoint
            rfpEntity.RFP_Link = $"/api/rfp/download/{rfpEntity.ID}";
            rfpEntity.RFP_Status = "Generated";
            rfpEntity.LastUpdatedDate = DateTime.UtcNow;
            rfpEntity.RFP_Prompt = finalizedTextContent; // Retained safely within your Azure SQL schema

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

        private (string SanitizedText, Dictionary<string, string> TokenMap) ApplySanitizationGateway(string rawText, string ClientName, string ClientEmail)
        {
            var tokenMap = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(rawText)) return (rawText, tokenMap);

            string sanitized = rawText;

            // Quick demo check of custom PII scrubbing patterns matching client variables
            if (sanitized.Contains(ClientName))
            {
                tokenMap.Add("[CLIENT_A]", ClientName);
                sanitized = sanitized.Replace(ClientName, "[CLIENT_A]");
            }

            if (sanitized.Contains(ClientEmail))
            {
                tokenMap.Add("[CLIENT_Email]", ClientEmail);
                sanitized = sanitized.Replace(ClientEmail, "[CLIENT_Email]");
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


    // A lightweight, custom token adapter that fulfills the AgentsClient parameter architecture
    // A lightweight, custom token adapter that fulfills the AgentsClient parameter architecture
    public class CustomTokenCredentialProvider : Azure.Core.TokenCredential
    {
        private readonly string _tenantId;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private static readonly System.Net.Http.HttpClient _httpClient = new System.Net.Http.HttpClient();

        public CustomTokenCredentialProvider(string tenantId, string clientId, string clientSecret)
        {
            _tenantId = tenantId;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        private async System.Threading.Tasks.Task<Azure.Core.AccessToken> FetchValidTokenAsync(System.Threading.CancellationToken cancellationToken)
        {
            var tokenUrl = $"https://login.microsoftonline.com/{_tenantId}/oauth2/v2.0/token";

            var requestBody = new System.Net.Http.FormUrlEncodedContent(new[]
            {
            new System.Collections.Generic.KeyValuePair<string, string>("grant_type", "client_credentials"),
            new System.Collections.Generic.KeyValuePair<string, string>("client_id", _clientId),
            new System.Collections.Generic.KeyValuePair<string, string>("client_secret", _clientSecret),
             new System.Collections.Generic.KeyValuePair<string, string>("scope", "https://cognitiveservices.azure.com/.default")
        });

            // EMERGENCY OVERRIDE FALLBACK: Hardcode an absolute string wipe to guarantee the payload changes
            //var dynamicBodyString = await requestBody.ReadAsStringAsync();
            //if (dynamicBodyString.Contains("https://azure.com"))
            //{
            //    var correctedRawPayload = dynamicBodyString.Replace("https%3A%2F%2Fazure.com", "https%3A%2F%2Fcognitiveservices.azure.com%2F.default");
            //    requestBody = new System.Net.Http.StringContent(correctedRawPayload, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
            //}

            var response = await _httpClient.PostAsync(tokenUrl, requestBody, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to retrieve token from Entra ID: {response.StatusCode} - {errorContent}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            using var doc = System.Text.Json.JsonDocument.Parse(jsonString);
            var root = doc.RootElement;

            string token = root.GetProperty("access_token").GetString();
            int expiresInSeconds = root.GetProperty("expires_in").GetInt32();

            // Pass a valid, decoded JWT token context back to the active client
            return new Azure.Core.AccessToken(token, DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds - 60));
        }

        public override Azure.Core.AccessToken GetToken(Azure.Core.TokenRequestContext requestContext, System.Threading.CancellationToken cancellationToken)
        {
            return FetchValidTokenAsync(cancellationToken).GetAwaiter().GetResult();
        }

        public override System.Threading.Tasks.ValueTask<Azure.Core.AccessToken> GetTokenAsync(Azure.Core.TokenRequestContext requestContext, System.Threading.CancellationToken cancellationToken)
        {
            return new System.Threading.Tasks.ValueTask<Azure.Core.AccessToken>(FetchValidTokenAsync(cancellationToken));
        }
    }


}
