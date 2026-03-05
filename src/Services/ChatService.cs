using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ZavaStorefront.Services
{
    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class ChatService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ChatService> _logger;
        private readonly string _requestUrl;
        private readonly string? _apiKey;
        private readonly bool _isConfigured;

        public ChatService(HttpClient httpClient, IConfiguration configuration, ILogger<ChatService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            var endpoint = configuration["AzureAIFoundry:Endpoint"]?.TrimEnd('/');
            _apiKey = configuration["AzureAIFoundry:ApiKey"];
            var deploymentName = configuration["AzureAIFoundry:DeploymentName"] ?? "phi-4";
            var apiVersion = configuration["AzureAIFoundry:ApiVersion"] ?? "2024-05-01-preview";

            _isConfigured = !string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(_apiKey);
            _requestUrl = _isConfigured
                ? $"{endpoint}/openai/deployments/{deploymentName}/chat/completions?api-version={apiVersion}"
                : string.Empty;
        }

        /// <summary>
        /// Sends the conversation history to the Phi-4 model and returns the assistant reply.
        /// </summary>
        /// <returns>
        /// A tuple where <c>Reply</c> contains the model response on success, or <c>Error</c> contains
        /// a user-friendly message when the request fails. Only one of the two will be non-null.
        /// </returns>
        public async Task<(string? Reply, string? Error)> SendMessageAsync(IReadOnlyList<ChatMessage> conversationHistory)
        {
            if (!_isConfigured)
            {
                _logger.LogWarning("Azure AI Foundry endpoint or API key is not configured");
                return (null, "Chat is not configured. Please set AzureAIFoundry:Endpoint and AzureAIFoundry:ApiKey in configuration.");
            }

            var messages = conversationHistory.Select(m => new { role = m.Role, content = m.Content });
            var requestBody = new { messages, max_tokens = 1024 };

            var json = JsonSerializer.Serialize(requestBody);
            using var request = new HttpRequestMessage(HttpMethod.Post, _requestUrl);
            request.Headers.Add("api-key", _apiKey);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending {MessageCount} message(s) to Phi-4 deployment", conversationHistory.Count);

            try
            {
                using var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Azure AI Foundry returned {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                    return (null, $"Error: The AI endpoint returned an unexpected response ({(int)response.StatusCode}).");
                }

                var responseJson = await response.Content.ReadAsStringAsync();

                try
                {
                    using var doc = JsonDocument.Parse(responseJson);
                    var content = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    _logger.LogInformation("Received response from Phi-4");
                    return (content ?? string.Empty, null);
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Failed to parse response from Azure AI Foundry");
                    return (null, "Error: Unable to parse the response from the AI endpoint.");
                }
                catch (IndexOutOfRangeException ex)
                {
                    _logger.LogError(ex, "Failed to parse response from Azure AI Foundry: unexpected array structure");
                    return (null, "Error: Unable to parse the response from the AI endpoint.");
                }
                catch (KeyNotFoundException ex)
                {
                    _logger.LogError(ex, "Failed to parse response from Azure AI Foundry: missing expected property");
                    return (null, "Error: Unable to parse the response from the AI endpoint.");
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogError(ex, "Failed to parse response from Azure AI Foundry: invalid JSON structure");
                    return (null, "Error: Unable to parse the response from the AI endpoint.");
                }
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Request to Azure AI Foundry timed out");
                return (null, "Error: The request to the AI endpoint timed out. Please try again.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to reach Azure AI Foundry endpoint");
                return (null, "Error: Unable to reach the AI endpoint. Please check the configuration and network connectivity.");
            }
        }
    }
}
