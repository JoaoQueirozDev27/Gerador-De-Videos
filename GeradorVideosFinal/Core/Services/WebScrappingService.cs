using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;
using Domain.Entities;
using PexelsDotNetSDK.Api;
using PexelsDotNetSDK.Models;
using System.Text.Json;

namespace Services
{
    public class WebScrappingService
    {
            public async Task<string> GetPage(string url)
            {
                HttpClient _httpClient = new HttpClient();

                HttpResponseMessage response = await _httpClient.GetAsync(url);

                string htmlContent = await response.Content.ReadAsStringAsync();
                return htmlContent;
            }

            public List<string> GetLinksOnPage(string htmlContent)
            {

                List<string> links = new List<string>();

                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                HtmlNodeCollection newsNodes = htmlDoc.DocumentNode.SelectNodes("//a[contains(@class, 'feed-post-body')]");

                foreach (HtmlNode node in newsNodes)
                {
                    links.Add(node.GetAttributeValue("href", string.Empty));
                }
                return links;
            }

            public List<string> GetLinksOfImages(string request, int start)
            {
                string apiKey = "AIzaSyAEFOTitg6jnmIJkOOSieC-jltyrpi_UgA";
                string cx = "135a4cbbae03d455e";

                string url = $"https://www.googleapis.com/customsearch/v1?q={Uri.EscapeDataString(request)}&cx={cx}&searchType=image&key={apiKey}&start={start}";

                using var client = new HttpClient();
                var response = client.GetStringAsync(url).Result;

                dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(response);
                var images = new List<string>();

                if (json.items != null)
                {
                    foreach (var item in json.items)
                    {
                        images.Add((string)item.link);
                    }
                }

                return images;
            }
        /*
            public async Task<News> Scrap(string Link)
            {
                try
                {
                    string html = await GetPage(Link);

                    var htmlDoc = new HtmlDocument();

                    htmlDoc.LoadHtml(html);

                    string title = htmlDoc.DocumentNode
                                        .SelectSingleNode("//h1[contains(@class, 'content-head__title')]").InnerText;

                    string text = string.Join("",
                                                    htmlDoc.DocumentNode.
                                                    SelectNodes("//p[contains(@class, 'content-text__container')]")
                                                    .Select(x => x.InnerText));

                    News News = new News();

                    News.title = title;
                    News.content = text;

                    return News;
                }
                catch (Exception ex)
                {
                    return null;
                }

            }
            public async Task<List<News>> GetNews(string Url)
            {
                string htmlContent = await GetPage(Url);

                List<string> links = GetLinksOnPage(htmlContent);

                List<News> AllNews = new List<News>();

                foreach (string link in links)
                {
                    if (!link.Contains("/noticia/")) continue;

                    News News = await Scrap(link);

                    if (News != null)
                        AllNews.Add(News);
                }

                return AllNews;
            }

        */
            public async Task<List<string>> GetUrlImages(string request, bool Context)
            {
                List<string> Urlimages = new List<string>();

                if (Context == false)
                {
                    var pexelsClient = new PexelsClient("lTaC0XiQSQbmxYf4DGSF77sfVFcKlZJCg9BKmj0dpkyuofQRedTbCIva");
                    PhotoPage result = await pexelsClient.SearchPhotosAsync(request);

                    result.photos.ToList().ForEach(photo =>
                    {
                        string url = photo.source.portrait;
                        Urlimages.Add(url);
                    });
                }

                else
                {
                    HttpClient httpClient = new HttpClient();

                    var response = await httpClient.GetAsync($"https://commons.wikimedia.org/w/api.php?action=query&format=json&generator=search&gsrsearch={Uri.EscapeDataString(request)}&gsrnamespace=6&gsrlimit=20&prop=imageinfo&iiprop=url");

                    string responseBody = await response.Content.ReadAsStringAsync();

                    var jsonResponse = JsonDocument.Parse(responseBody);

                    if (jsonResponse.RootElement.TryGetProperty("query", out JsonElement queryElement) &&
                            queryElement.TryGetProperty("pages", out JsonElement pagesElement))
                    {
                        foreach (JsonProperty page in pagesElement.EnumerateObject())
                        {
                            if (page.Value.TryGetProperty("imageinfo", out JsonElement imageInfoElement))
                            {
                                foreach (JsonElement imageInfo in imageInfoElement.EnumerateArray())
                                {
                                    if (imageInfo.TryGetProperty("url", out JsonElement urlElement))
                                    {
                                        string imageUrl = urlElement.GetString();
                                        Urlimages.Add(imageUrl);
                                    }
                                }
                            }
                        }
                    }
                }
                return Urlimages;
            }        

            public void DownLoadImage(string url, string path)
            {
            
            }
    }
}
