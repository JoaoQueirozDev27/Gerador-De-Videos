using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;


namespace Services.ContentSources.RssContentSource
{
    public class G1RssContentSource : IContentSource<RssContentSourceRequest>
    {
        public string Name => "RSS de notícias do G1";

        public Type RequestType => typeof(RssContentSourceRequest);

        public Type ContentType => typeof(Content);

        public async Task<List<IContent>> GetContent(RssContentSourceRequest request)
        {
            if(request is RssContentSourceRequest)
            {
                using HttpClient http = new HttpClient();

                string xml = await http.GetStringAsync((request as RssContentSourceRequest).url);

                XDocument doc = XDocument.Parse(xml);

                List<IContent> contents = new List<IContent>();

                int count = 1;
                foreach (var item in doc.Descendants("item"))
                {
                    string title = item.Element("title")?.Value ?? "";
                    string link = item.Element("link")?.Value ?? "";
                    string description = item.Element("description")?.Value ?? "";

                    description = System.Net.WebUtility.HtmlDecode(description);

                    contents.Add(new G1RssContentSourceResponse { Id = count, Title = title, Link = link, Description = description });
                    count++;

                }
                return contents;
            }
            throw new ArgumentException("Invalid request type");
        }

        public Task<List<IContent>> GetContent(object request)
        {
            return GetContent((RssContentSourceRequest)request);
        }
    }
}
