using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using FFMpegCore;
using FFMpegCore.Enums;

namespace Services
{
    public class VideoService
    {
        public VideoService()
        {
            GlobalFFOptions.Configure(opt =>
            {
                opt.BinaryFolder = @"C:\ffmpeg\bin";
                opt.TemporaryFilesFolder = Path.Combine(Path.GetTempPath(), "ffmpeg");
            });
        }

        public double GetDurationMs(string filePath)
        {
            var info = FFProbe.Analyse(filePath);
            return info.Duration.TotalMilliseconds;
        }

        public async Task ImageWithAudioAsync(byte[] imageBytes, string audioPath, string outputPath)
        {
            string imgTemp = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpg");
            await File.WriteAllBytesAsync(imgTemp, imageBytes);

            try
            {
                await FFMpegArguments
                    .FromFileInput(imgTemp)
                    .AddFileInput(audioPath)
                    .OutputToFile(outputPath, true, o => o
                        .WithVideoCodec(VideoCodec.LibX264)
                        .WithAudioCodec(AudioCodec.Aac)
                        .ForceFormat("mp4")
                        .WithCustomArgument("-shortest"))
                    .ProcessAsynchronously();

                //return outputPath;
            }
            catch
            {
                throw new Exception("Erro ao criar vídeo a partir da imagem e áudio.");
            }
            finally
            {
                if (File.Exists(imgTemp))
                    File.Delete(imgTemp);
            }
        }

        public async Task ImageAsync(byte[] imageBytes, double duration, string outputPath)
        {
            string imgTemp = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".jpeg");
            await File.WriteAllBytesAsync(imgTemp, imageBytes);

            try
            {
                await FFMpegArguments
                    .FromFileInput(imgTemp, false, options => options
                    .WithCustomArgument("-loop 1"))
                    .OutputToFile(outputPath, overwrite: true, options => options
                    .WithVideoCodec(VideoCodec.LibX264)
                    .ForceFormat("mp4")
                    .WithCustomArgument("-preset ultrafast")      
                    .WithCustomArgument("-crf 28")               
                    .WithCustomArgument("-tune stillimage")      
                    .WithCustomArgument("-vf scale=trunc(iw/2)*2:trunc(ih/2)*2,format=yuv420p")
                    .WithCustomArgument("-pix_fmt yuv420p")
                    .WithCustomArgument("-r 1")                  
                    .WithDuration(TimeSpan.FromSeconds(duration)))
                    .NotifyOnProgress(progress => {
                        Console.WriteLine($"Renderizando... {progress.Milliseconds}%");
                    })
                    .NotifyOnOutput(output => {    
                        Console.WriteLine($"{output.Trim()}");
                    })
                    .ProcessAsynchronously();

            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao criar vídeo a partir da imagem.", ex);
            }
            finally
            {
                if (File.Exists(imgTemp))
                    File.Delete(imgTemp);
            }
        }
        public async Task VideoWithDuration(byte[] VideoBytes, double duration, string outputPath)
        {
            string imgTemp = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
            await File.WriteAllBytesAsync(imgTemp, VideoBytes);

            try
            {

                await FFMpegArguments
                    .FromFileInput(imgTemp)
                    .OutputToFile(outputPath, true, o => o
                        .WithVideoCodec(VideoCodec.LibX264)
                        .ForceFormat("mp4")
                        .WithCustomArgument("-shortest")
                        .WithDuration(TimeSpan.FromMilliseconds(duration)))
                    .ProcessAsynchronously();

                //return outputPath;
            }
            catch
            {
                throw new Exception("Erro ao criar vídeo a partir da imagem e áudio.");
            }
            finally
            {
                if (File.Exists(imgTemp))
                    File.Delete(imgTemp);
            }
        }

        public async Task VideoWithoutDuration(byte[] VideoBytes, string outputPath)
        {
            string imgTemp = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".mp4");
            await File.WriteAllBytesAsync(imgTemp, VideoBytes);


            try
            {
                await FFMpegArguments
                    .FromFileInput(imgTemp)
                    .OutputToFile(outputPath, true, o => o
                        .WithVideoCodec(VideoCodec.LibX264)
                        .ForceFormat("mp4")
                        .WithCustomArgument("-shortest"))
                    .ProcessAsynchronously();

                //return outputPath;
            }
            catch
            {
                throw new Exception("Erro ao criar vídeo a partir da imagem e áudio.");
            }
            finally
            {
                if (File.Exists(imgTemp))
                    File.Delete(imgTemp);
            }
        }

        public async Task ConcatVideosAsync(string[] inputs, string output)
        {
            await FFMpegArguments
                .FromConcatInput(inputs)
                .OutputToFile(output, true, o => o.WithCopyCodec())
                .ProcessAsynchronously();
        }

        public async Task AddMainAudioAsync(string video, string audio, string output)
        {
            await FFMpegArguments
                .FromFileInput(video)
                .AddFileInput(audio)
                .OutputToFile(output, true, o => o
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithCustomArgument("-map 0:v:0 -map 1:a:0 -shortest"))
                .ProcessAsynchronously();
        }

        public async Task<string> OverlayLayerAsync(string baseVideo, string overlayVideo, double startSec, string output)
        {
            double duration = FFProbe.Analyse(overlayVideo).Duration.TotalSeconds;

            string filter =
                $"[1:v]setpts=PTS+{startSec}/TB[ov];" +
                $"[0:v][ov]overlay=0:0:enable='between(t,{startSec},{startSec + duration})'";

            await FFMpegArguments
                .FromFileInput(baseVideo)
                .AddFileInput(overlayVideo)
                .OutputToFile(output, true, o => o
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithCustomArgument($"-filter_complex \"{filter}\""))
                .ProcessAsynchronously();

            return output;
        }

        public async Task<string> OverlayLayerWithDurationAsync(string baseVideo, string overlayVideo, double startSec,double duration, string output)
        {
            //double duration = FFProbe.Analyse(overlayVideo).Duration.TotalSeconds;

            string filter =
                $"[1:v]setpts=PTS+{startSec}/TB[ov];" +
                $"[0:v][ov]overlay=0:0:enable='between(t,{startSec},{startSec + duration})'";

            await FFMpegArguments
                .FromFileInput(baseVideo)
                .AddFileInput(overlayVideo)
                .OutputToFile(output, true, o => o
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithCustomArgument($"-filter_complex \"{filter}\""))
                .ProcessAsynchronously();

            return output;
        }

        // EXTRAIR AUDIO
        public async Task ExtractAudioAsync(string video, string wavOut)
        {
            await FFMpegArguments
                .FromFileInput(video)
                .OutputToFile(wavOut, true, o => o
                    .WithAudioCodec(AudioCodec.Aac)
                    .DisableChannel(Channel.Video))
                .ProcessAsynchronously();
        }

        public void CleanupTemp(string path)
        {
            foreach (var f in Directory.GetFiles(path, "temp*.mp4"))
            {
                try { File.Delete(f); }
                catch { }
            }
        }
    }
}