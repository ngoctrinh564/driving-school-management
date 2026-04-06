using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Configuration;
using driving_school_management.Configs;

public class AiChatService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public AiChatService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        _apiKey = configuration["AiSettings:ApiKey"];
        _model = configuration["AiSettings:Model"];
        var baseUrl = configuration["AiSettings:BaseUrl"];

        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> AskAsync(string userMessage)
    {
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = SystemPrompt.GPLX_VIETNAM
                },
                new
                {
                    role = "user",
                    content = userMessage
                }
            }
        };

        var json = JsonConvert.SerializeObject(requestBody);

        var response = await _httpClient.PostAsync(
            "chat/completions",
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        var raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception(raw);

        dynamic result = JsonConvert.DeserializeObject(raw);

        return (string)result.choices[0].message.content;
    }
}