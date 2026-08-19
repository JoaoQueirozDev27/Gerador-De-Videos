using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ContentSources.AiContentSource
{
    public class AiContentSource : IContentSource
    {
        public async Task<List<Content>> getContent(object request)
        {
            if(request is AiContentSourceRequest)
            {

                AiService aiService = new AiService();

                string result = await aiService.SendPrompt((request as AiContentSourceRequest).getPrompt());

                return new List<Content>
                {
                    new Content { Id = 1, Title = "AiContentSorceResult", result = result }
                };
            }
            else
            {
                throw new ArgumentException("Invalid request type");
            }
        }
    }
}
