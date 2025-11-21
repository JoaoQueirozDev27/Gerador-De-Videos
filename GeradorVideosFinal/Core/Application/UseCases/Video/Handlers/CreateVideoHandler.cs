using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.interfaces;
using Core.Domain;

namespace Application.UseCases.Video.Handlers
{
    public class CreateVideoHandler
    {
        private readonly IVideoService _videoService;
        private readonly IAudioService _audioService;
        private readonly IImageService _imageService;
        private readonly IAiService _aiService;

        public CreateVideoHandler(IVideoService videoService, IAudioService audioService, IImageService imageService, IAiService aiService)
        {
            _videoService = videoService;
            _audioService = audioService;
            _imageService = imageService;
            _aiService = aiService;
        }

        public async Task Handle(string topic)
        {

            //string script = await _aiService.GetSummary(topic);
            
            //byte[] audioBytes = await _audioService.GenerateAudio(script);
            
            //double audioDuration = _audioService.GetAudioDuration(audioBytes);
            
            //List<byte[]> images = new List<byte[]>();

            //var prompts = script.Split("-*-");
                        
            //foreach (var prompt in prompts)
            //{
            //    byte[] imageBytes = await _imageService.GenerateImage(prompt);
            //    images.Add(imageBytes);
            //}
            
            //await _videoService.GenerateVideo("");
        }

    }
}
