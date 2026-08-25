using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.ContentSources.AiContentSource
{
    public class AiContentSource : IContentSource<AiContentSourceRequest>  
    {
        public string Name => "Inteligência Artificial";
        public Type RequestType => typeof(AiContentSourceRequest);
        public Type ContentType => typeof(AiContentSourceResponse);
        public async Task<List<IContent>> GetContent(AiContentSourceRequest request)
        {
            if (request is AiContentSourceRequest)
            {
                if (request is not null)
                {
                    AiService aiService = new AiService();

                    string result = await aiService.SendPrompt(request.getPrompt());

                    return new List<IContent>
                    {
                        new AiContentSourceResponse { Id = 1, Title = "AiContentSorceResult", Result = result }
                    };
                }
                else
                {
                    throw new ArgumentNullException("Request cannot be null");
                }
            }
            else
            {
                throw new ArgumentException("Invalid request type");
            }
        }
        public Task<List<IContent>> GetContent(object request)
        {
            return GetContent((AiContentSourceRequest)request);
        }
    }
}
