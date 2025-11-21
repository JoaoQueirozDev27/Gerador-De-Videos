using Application.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Audio.TextToSpeech
{
    public class TextToSpeechHandler
    {
        private readonly IAudioService _audioService;

        public TextToSpeechHandler(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public async Task<byte[]> Handle(TextToSpeechCommand command)
        {
            byte[] audioBytes = await _audioService.GenerateFinalAudio(command.text);
            return audioBytes;
        }

    }

}
