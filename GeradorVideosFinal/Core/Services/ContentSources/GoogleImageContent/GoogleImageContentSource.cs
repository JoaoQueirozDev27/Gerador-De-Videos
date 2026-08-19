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
    public class GoogleImageContentSource : IContentSource
    {
        public async Task<List<Content>> getContent(object request)
        {
            if (request is GoogleImageContentRequest)
            {
                GoogleImage googleImage = new GoogleImage();

                List<string> urlsImages = await googleImage.Search((request as GoogleImageContentRequest).query);

                List<Content> Images = new List<Content>();

                int count = 0;

                foreach (string url in urlsImages) { 
                    Images.Add(new Content { Id = count, Title = "GoogleImageContentSourceResult", result = url });
                    count++;
                }

                return Images;
            }
            else
            {
                throw new ArgumentException("Invalid request type");
            }

        }        
    }
}
