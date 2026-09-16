using Azure;
using Azure.AI.Projects;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using ContractPulseAI.API.Services.Interface;
using System.Text;

namespace ContractPulseAI.API.Services.Implementation
{
    public class SowGenerationClient : ISowGenerationClient
    {
        private readonly AgentsClient _agentsClient;
        private readonly SearchClient _searchClient;
        private readonly string _agentId;

        public SowGenerationClient(IConfiguration configuration)
        {
            // 1. Extract settings from appsettings.json
            string connectionString = configuration["AzureFoundrySettings:ProjectConnectionString"]
                ?? throw new InvalidOperationException("Project Connection String is missing from configurations.");

            _agentId = configuration["AzureFoundrySettings:AgentId"]
                ?? throw new InvalidOperationException("Agent ID is missing from configurations.");

            string foundryApiKey = configuration["AzureFoundrySettings:ApiKey"]
                ?? throw new InvalidOperationException("Foundry API Key is missing from configurations.");

            string searchEndpoint = configuration["AzureServices:SearchEndpoint"]
                ?? "https://windows.net";

            string searchApiKey = configuration["AzureServices:SearchApiKey"]
                ?? throw new InvalidOperationException("Search API Key is missing from configurations.");

            string searchIndexName = configuration["AzureServices:SearchIndexName"]
                ?? "search-1789470637458"; // Set to your live active index name!

            // 2. Initialize the internal clients using the custom token provider workaround to prevent duplicate metadata errors
            var cleanCredential = new CustomTokenCredentialProvider(foundryApiKey);
            _agentsClient = new AgentsClient(connectionString, cleanCredential);
            _searchClient = new SearchClient(new Uri(searchEndpoint), searchIndexName, new Azure.AzureKeyCredential(searchApiKey));
        }

        /// <summary>
        /// Runs manual RAG text retrieval from Azure AI Search and triggers your gpt-5-mini cloud agent conversation thread loop.
        /// </summary>
        public async Task<string> ExecuteRfpToSowPipelineAsync(string rfpPrompt, string budget, string duration, string resource, string? additionalPrompt)
        {
            // A. Execute manual corporate context lookup in Azure AI Search
            string legalBoilerplateClauses = await RetrieveMasterContractTemplatesAsync(rfpPrompt);

            // B. Package user form fields and RAG data directly into a clean user payload string (No static prompts in code)
            var dynamicContextBuilder = new StringBuilder();
            dynamicContextBuilder.AppendLine("### RETRIEVED MASTER CONTRACT TEMPLATES & CLAUSES (RAG) ###");
            dynamicContextBuilder.AppendLine(legalBoilerplateClauses);
            dynamicContextBuilder.AppendLine();
            dynamicContextBuilder.AppendLine("### DYNAMIC COMMERCIAL INPUT PARAMETERS ###");
            dynamicContextBuilder.AppendLine($"- Financial Budget: {budget}");
            dynamicContextBuilder.AppendLine($"- Project Duration: {duration}");
            dynamicContextBuilder.AppendLine($"- Total Allocated Resources Count: {resource}");
            dynamicContextBuilder.AppendLine();
            dynamicContextBuilder.AppendLine("### INITIAL CLIENT REQUIREMENTS ###");
            dynamicContextBuilder.AppendLine(rfpPrompt);

            if (!string.IsNullOrWhiteSpace(additionalPrompt))
            {
                dynamicContextBuilder.AppendLine();
                dynamicContextBuilder.AppendLine($"### ADDITIONAL CUSTOM DEMANDS ###");
                dynamicContextBuilder.AppendLine(additionalPrompt);
            }

            // C. Provision a secure conversational transaction Thread session container on Azure Foundry
            Response<AgentThread> threadResponse = await _agentsClient.CreateThreadAsync();
            string threadId = threadResponse.Value.Id;

            // Push our fully augmented context string onto the active thread
            await _agentsClient.CreateMessageAsync(threadId, MessageRole.User, dynamicContextBuilder.ToString());

            // D. Fire up the execution runner on your gpt-5-mini instance using your cloud-configured Agent ID
            Response<ThreadRun> runResponse = await _agentsClient.CreateRunAsync(threadId, _agentId);
            string runId = runResponse.Value.Id;

            // E. Polling loop: Wait for cloud completion
            ThreadRun currentRun;
            do
            {
                await Task.Delay(TimeSpan.FromSeconds(1)); // Check every 1 second for instant hackathon UI feedback
                Response<ThreadRun> checkResponse = await _agentsClient.GetRunAsync(threadId, runId);
                currentRun = checkResponse.Value;
            }
            while (currentRun.Status == RunStatus.Queued || currentRun.Status == RunStatus.InProgress);

            // F. Parse out the resulting compiled text response from the message tracking history layout
            if (currentRun.Status == RunStatus.Completed)
            {
                Response<PageableList<ThreadMessage>> listResponse = await _agentsClient.GetMessagesAsync(threadId);
                var assistantMessage = listResponse.Value.Data.FirstOrDefault(m => m.Role == MessageRole.Agent);

                if (assistantMessage != null)
                {
                    foreach (var contentItem in assistantMessage.ContentItems)
                    {
                        if (contentItem is MessageTextContent textItem)
                        {
                            return textItem.Text; // Returns raw compiled contract text layout string safely
                        }
                    }
                }
            }

            throw new Exception($"SOW Generation Client thread execution loop halted with fatal cloud error status: {currentRun.Status}");
        }

        /// <summary>
        /// Manual search query engine pulling the top 3 corresponding legal items out of Azure AI Search columns.
        /// </summary>
        private async Task<string> RetrieveMasterContractTemplatesAsync(string searchInputQuery)
        {
            var searchBuilder = new StringBuilder();
            try
            {
                var searchOptions = new SearchOptions { Size = 3 };
                searchOptions.Select.Add("content");
                searchOptions.Select.Add("metadata_title");

                var searchResultResponse = await _searchClient.SearchAsync<SearchDocument>(searchInputQuery, searchOptions);

                await foreach (var result in searchResultResponse.Value.GetResultsAsync())
                {
                    if (result.Document.TryGetValue("content", out var contentText))
                    {
                        searchBuilder.AppendLine("--- Standard Corporate Clause Match ---");
                        searchBuilder.AppendLine(contentText?.ToString());
                    }
                }
            }
            catch (Exception)
            {
                // Baseline architectural fallback backup clause string in case of connection drop
                searchBuilder.AppendLine("Standard Clause Reference: Commercial processing terms default strictly to Net 30 windows. Resource vacancy replacements match a maximum 10 business day turnaround.");
            }

            return searchBuilder.Length > 0 ? searchBuilder.ToString() : "No company master clauses found matching request parameters.";
        }
    }
}
