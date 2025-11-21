using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.interfaces
{
    public interface IAudioService
    {
        public Task<byte[]> GenerateFinalAudio(string text);
        public double GetAudioDuration(string FilePath);
        public Task GenerateTemporaryAudio(string text,string modelPath,string path);
    }
}
