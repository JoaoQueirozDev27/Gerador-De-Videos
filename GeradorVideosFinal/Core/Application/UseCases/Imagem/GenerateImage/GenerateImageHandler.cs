using Application.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Imagem.GenerateImage
{
    public class GenerateImageHandler
    {
        private readonly IImageService _ImageService;
        public GenerateImageHandler(IImageService imageService)
        {
            _ImageService = imageService;
        }
        public async Task<byte[]> Handle(GenerateImageCommand command)
        {
            byte[] imageBytes = await _ImageService.GenerateImage(command.prompt);
            return imageBytes;
        }
    }
}
