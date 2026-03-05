using Application.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.IA.CreatePromptToImage
{
    public class CreatePromptToImageHandler
    {
        private readonly IAiService _aiService;
        public CreatePromptToImageHandler(IAiService aiService)
        {
            _aiService = aiService;
        }

        public async Task<string> Handle(CreatePromptToImageCommand command)
        {
            //string prompt = await _aiService.TransformText(command.content);
            return "";
        }
    }
}
