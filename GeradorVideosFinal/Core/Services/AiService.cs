using Application.interfaces;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace Services
{
    public class AiService : IAiService
    {
        HttpClient _client = new HttpClient();
        
        public AiService()
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "Coloque a ApiKey do groq aqui");
        }

        public async Task<string> SendPrompt(string prompt)
        {
            var requestJson = JsonSerializer.Serialize(new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[]{
                    new { role = "user", content = prompt }
                }
            });

            var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("https://api.groq.com/openai/v1/chat/completions", requestContent);
            var result = await response.Content.ReadAsStringAsync();

            var responseJson = System.Text.Json.JsonDocument.Parse(result);

            string responseContent = responseJson.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

            return responseContent;
        }

        public async Task<string> TransformText(string prompt, string content)
        {
            var fullPrompt = $"{prompt} + {content}";
            return await SendPrompt(fullPrompt);
        }

        public async Task<string> ScrapeWithAI(string url)
        {
            var client  = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://scrape.serper.dev");
            request.Headers.Add("X-API-KEY", "2c7445a0d2969c203dd14937df027ed1e1ed2464");
            var content = new StringContent($"{{\"url\":\"{url}\"}}", Encoding.UTF8, "application/json");

            request.Content = content;
            var response = await client.SendAsync(request);
            string responseJson = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode();

            //var jsonDoc = JsonDocument.Parse(responseJson);
            //responseJson = jsonDoc.RootElement.GetProperty("content").GetString();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
