using Application.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Audio.CalcSrtDuraton
{
    public class CalcSrtDurationHandler
    {
        private readonly IAudioService _audioService;
        public CalcSrtDurationHandler(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public async Task<double> Handle(CalcSrtDurationCommand command)
        {
            await _audioService.GenerateTemporaryAudio(command.text,command.modelPath, command.path);
            double duration = _audioService.GetAudioDuration(command.path);

            return duration;
        }
    }
}
