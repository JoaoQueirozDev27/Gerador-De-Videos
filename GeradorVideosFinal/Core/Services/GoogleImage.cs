using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;

namespace Services
{
    public class GoogleImage
    {
        private readonly HttpClient _httpClient;

        public GoogleImage()
        {
            _httpClient = new HttpClient();

            _httpClient.DefaultRequestHeaders.Add(
                "User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                "(KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36"
            );
        }

        public async Task<List<string>> Search(string query)
        {
            string url =
                $"https://www.google.com/search?tbm=isch&q={WebUtility.UrlEncode(query)}";

            string html = await _httpClient.GetStringAsync(url);

            var document = new HtmlDocument();
            document.LoadHtml(html);

            var images = new List<string>();

            foreach (var img in document.DocumentNode.SelectNodes("//img"))
            {
                string? src = img.GetAttributeValue("src", null);

                if (!string.IsNullOrEmpty(src))
                {
                    images.Add(src);
                }
            }

            return images;
        }
    }
}
