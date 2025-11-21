using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;
using Core.Domain.Entities;
using Domain.Entities;

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
        
    }
}
