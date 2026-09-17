using Azure.AI.Extensions.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using ContractPulseAI.API.Services.Interface;
using System.Text;

namespace ContractPulseAI.API.Services.Implementation
{
    public class SowGenerationClient : ISowGenerationClient
    {
        private readonly ProjectOpenAIClient _agentsClient;
        private readonly SearchClient _searchClient;
        private readonly string _agentId;
        private readonly string _searchEndpoint;
        private readonly string _searchApiKey;
        private readonly string _searchIndexName;

        public SowGenerationClient(IConfiguration configuration, ProjectOpenAIClient agentsClient)
        {   
            // 1. Inject the working, centrally authenticated client provided by Program.cs
            _agentsClient = agentsClient;

            // 2. Pull down remaining infrastructure settings safely
            _agentId = configuration["AzureFoundrySettings:AgentId"]
                ?? throw new InvalidOperationException("Agent ID is missing from configurations.");

            _searchEndpoint = configuration["AzureServices:SearchEndpoint"]
                ?? "https://windows.net";

            _searchApiKey = configuration["AzureServices:SearchApiKey"]
                ?? throw new InvalidOperationException("Search API Key is missing from configurations.");

            _searchIndexName = configuration["AzureServices:SearchIndexName"]
                ?? "search-1789470637458";

            // 3. Initialize the AI search client natively
            _searchClient = new SearchClient(new System.Uri(_searchEndpoint), _searchIndexName, new Azure.AzureKeyCredential(_searchApiKey));
        }

        //public async Task<string> ExecuteRfpToSowPipelineAsync(string rfpPrompt, string budget, string duration, string resource, string? additionalPrompt)
        /// <summary>
        /// Runs manual RAG text retrieval from Azure AI Search and triggers your gpt-5-mini cloud agent conversation thread loop.
        /// </summary>
        public async Task<string> ExecuteRfpToSowPipelineAsync(string rfpPrompt)
        {
            // A. Execute manual corporate context lookup in Azure AI Search
            string legalBoilerplateClauses = await RetrieveMasterContractTemplatesAsync(rfpPrompt);

            try
            {
                ProjectResponsesClient responsesClient = _agentsClient.GetProjectResponsesClientForAgent(_agentId); // Ensure the agent client is ready for conversation
                var responseResult = await responsesClient.CreateResponseAsync(rfpPrompt);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error during RAG and agent execution: {ex.Message}");
            }

            return null;
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
