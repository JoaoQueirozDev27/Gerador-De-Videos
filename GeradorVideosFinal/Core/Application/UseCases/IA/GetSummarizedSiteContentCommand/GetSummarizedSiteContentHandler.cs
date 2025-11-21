using Application.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.IA.GetSummarizedSiteContentCommand
{
    public class GetSummarizedSiteContentHandler 
    {
        private readonly IAiService aiService;

        public GetSummarizedSiteContentHandler(IAiService aiService)
        {
            this.aiService = aiService;
        }

        public async Task<string> Handle(GetSummarizedSiteContentCommand command)
        {
            string SiteContent =
                await aiService
                .ScrapeWithAI(command.url);
            
            string response =
                await aiService
                .GetSummary(SiteContent);

            var responseJson = System.Text.Json.JsonDocument.Parse(response);

            string responseContent = responseJson.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

            return responseContent;
        }
    }
}
