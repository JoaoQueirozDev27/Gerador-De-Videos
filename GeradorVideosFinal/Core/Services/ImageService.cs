using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.interfaces;

namespace Services
{
    public class ImageService : IImageService
    {
        public async Task<byte[]> GenerateImage(string prompt)
        {
            string apiKey = "sc-23a3a89f0d9170bdec377190b357935417257e0d37303dfdec90d391c7923a33";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var requestBody = new
            {
                prompt = prompt,
                model_id = "0a99668b-45bd-4f7e-aa9c-f9aaa41ef13b",
                width = 512,
                height = 912
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            Console.WriteLine("Generating image with prompt: " + prompt);

            var response = await client.PostAsync($"https://api.stablecog.com/v1/image/generation/create", content);
            string json = await response.Content.ReadAsStringAsync();

            Console.WriteLine(json);

            using JsonDocument doc = JsonDocument.Parse(json);

            string url = doc.RootElement.GetProperty("outputs")[0].GetProperty("url").GetString();

            byte[] imageBytes = await client.GetByteArrayAsync(url);

            return imageBytes;
        }
    }
}
