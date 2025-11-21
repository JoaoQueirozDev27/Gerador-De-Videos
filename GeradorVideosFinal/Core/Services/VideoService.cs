using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.interfaces;


namespace Services
{
    public class VideoService : IVideoService
    {
        public async Task GenerateVideo(string arguments)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var processo = new Process { StartInfo = startInfo };

            // captura saída assíncrona sem travar o buffer
            processo.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Console.WriteLine("[ffmpeg err] " + e.Data);
            };

            processo.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    Console.WriteLine("[ffmpeg out] " + e.Data);
            };

            processo.Start();
            processo.BeginErrorReadLine();
            processo.BeginOutputReadLine();

            await processo.WaitForExitAsync();

            if (processo.ExitCode != 0)
            {
                throw new Exception($"FFmpeg falhou com código {processo.ExitCode}.");
            }

            Console.WriteLine("FFmpeg terminou com sucesso!");
        }


    }
}
