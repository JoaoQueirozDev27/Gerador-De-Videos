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
    public class G1RssContentSource : IContentSource
    {
        public async Task<List<Content>> getContent(object request)
        {
            if(request is RssContentSourceRequest)
            {

                using var http = new HttpClient();

                string xml = await http.GetStringAsync((request as RssContentSourceRequest).url);

                XDocument doc = XDocument.Parse(xml);

                List<Content> contents = new List<Content>();

                int count = 1;
                foreach (var item in doc.Descendants("item"))
                {
                    string titulo = item.Element("title")?.Value ?? "";
                    string link = item.Element("link")?.Value ?? "";
                    string descricao = item.Element("description")?.Value ?? "";

                    descricao = System.Net.WebUtility.HtmlDecode(descricao);

                    contents.Add(new Content { Id = count, Title = titulo, result = descricao });
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
