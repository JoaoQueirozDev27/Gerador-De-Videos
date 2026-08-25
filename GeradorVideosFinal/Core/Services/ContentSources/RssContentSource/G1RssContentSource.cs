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

        public async Task<List<IContent>> getContent(RssContentSourceRequest request)
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
                    string titulo = item.Element("title")?.Value ?? "";
                    string link = item.Element("link")?.Value ?? "";
                    string descricao = item.Element("description")?.Value ?? "";

                    descricao = System.Net.WebUtility.HtmlDecode(descricao);

                    contents.Add(new G1RssContentSourceResponse { Id = count, Title = titulo, result = descricao });
                    count++;

                }
                return contents;
            }
            else
            {
                throw new ArgumentException("Invalid request type");
            }
        }
    }
}
