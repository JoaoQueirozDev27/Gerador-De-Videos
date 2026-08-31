using Domain.Entities;
using Domain.Interfaces;
using HtmlAgilityPack;
using Services.ContentSources.AiContentSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Services.ContentSources.GoogleImageContent
{
    public class GoogleImageContentSource : IContentSource<GoogleImageContentRequest>
    {
        public string Name => "Google Images";

        public Type RequestType => typeof(GoogleImageContentRequest);

        public Type ContentType => typeof(GoogleImageContentResponse);

        public async Task<List<IContent>> GetContent(object request)
        {           
            return await GetContent((GoogleImageContentRequest)request);
        }

        public async Task<List<IContent>> GetContent(GoogleImageContentRequest request)
        {
            
            GoogleImage googleImage = new GoogleImage();

            List<string> urlsImages = await googleImage.Search(request.query);

            List<IContent> Images = new List<IContent>();

            int count = 0;

            foreach (string url in urlsImages)
            {
                Images.Add(new GoogleImageContentResponse() { Id = count, Title = "", Result=url});
                count++;
            }

            return Images;

        }
    }
}
