using System;
using System.Collections.Generic;
using Services;
using Application.interfaces;
using Application.UseCases;
using Core.Domain.Entities;
using static System.Net.Mime.MediaTypeNames;
using Application.UseCases.IA.GetSummarizedSiteContentCommand;
using Application.UseCases.Audio.TextToSpeech;
using Application.UseCases.Imagem.GenerateImage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Application.UseCases.IA.CreatePromptToImage;
using Application.UseCases.Audio.CalcSrtDuraton;
using Spectre.Console;
using Spectre.Console.Rendering;
using Services.Factories;
using System.Drawing;
using Core.Domain.Entities;
using Domain.Entities;

namespace Presentation
{
    class Program
    {

        static IAiService aiService = new AiService();
        static IAudioService audioService = new AudioService();
        static IVideoService videoService = new VideoService();
        static IMediaManager mediaManager = YoutubeManagerFactory.CreateYoutubeManagerInstance();
        static WebScrappingService webScrappingService = new WebScrappingService();

        static async Task Main(string[] args)
        {
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            AnsiConsole.Write(new FigletText("Video Creator").Centered().Color(Spectre.Console.Color.Green));

            Log("[yellow]Iniciando o processo de criação de vídeos...[/]");

            CreateDirectoryIfNotExists("C:\\Users\\Administrador\\Desktop\\G1");

            #region Processing Video in Paralell

            List<News> AllNews = await webScrappingService.GetNews("https://g1.globo.com/");

            List<Task> tasks = new List<Task>();

            int i = 1;

            tasks.Add(ProcessVideo(AllNews[0].content, i));

            //AllNews.ForEach(x => 
            //{
            //    tasks.Add(ProcessVideo(x.content,i));
            //    i++;
            //});

            await Task.WhenAll(tasks);
            #endregion
        }
        public static async Task ProcessVideo(string SiteContent,int i)
        {
            string path = $"C:\\Users\\Administrador\\Desktop\\G1\\Video{i}".Replace("\r","").Replace("\n","");

            CreateDirectoryIfNotExists(path);

            bool confirm = false;
            List<string> lines = new List<string>();

            while (confirm == false) {
                string response = await aiService.GenerateVideoScript(SiteContent);

                var responseJson = System.Text.Json.JsonDocument.Parse(response);

                string? responseContent = responseJson.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

                Log($"\r\n[green]Roteiro gerado com sucesso![/]");
                Log($"\r\n[yellow]{responseContent}[/]");

                #region SRT Generation

                //int j = 0;
                //TimeSpan tempoInicial = TimeSpan.FromSeconds(0);
                //string srtContent = "";
                //foreach (var line in responseContent.Split("~")[2..])
                //{ 
                //    if (String.IsNullOrEmpty(line)) continue;

                //    srtContent += $"{j + 1}\r\n\r\n";
                //    double duration = await CalcSrtDuration(line, path);
                //    TimeSpan tempoFinal = tempoInicial.Add(TimeSpan.FromSeconds(duration));
                //    srtContent += $"{tempoInicial:hh\\:mm\\:ss\\,fff} --> {tempoFinal:hh\\:mm\\:ss\\,fff}\r\n\r\n";
                //    srtContent += line.Trim();
                //    srtContent += "\r\n\r\n";
                //    tempoInicial = tempoFinal;
                //    j++;
                //}

                //File.WriteAllText(path + "\\subtitles.srt", srtContent);

                #endregion

                lines = responseContent.Replace("\n\n","\n").Split("\n").ToList();

                if(lines.Count == 0)
                {
                    Log($"\r\n[red]O roteiro gerado não está no formato esperado. Tentando novamente...[/]");
                    continue;
                }

                Log($"\r\n[yellow]Título do vídeo: [/][green]{lines[0]}[/]");

                Log($"\r\n[yellow]Conteúdo do vídeo: [/][green]{string.Join(" ",lines[2..])}[/]");

                confirm = AnsiConsole.Confirm("Deseja continuar com a geração do vídeo?");
            }
            
            File.WriteAllText($"{path}\\SummarizedContent.txt", string.Join(" ",lines[2..]));

            await audioService.GenerateTemporaryAudio(string.Join(" ", lines[2..]), "C:\\KokoroModel\\kokoro.onnx", path + "\\GeneratedAudio.wav");

            await videoService
                .GenerateVideo(@$"-y -i {path + "\\GeneratedAudio.wav"} -filter:a loudnorm -codec:a libmp3lame -qscale:a 2 {path + "\\GeneratedAudio.mp3"}");

            string caminhoLegenda = Path.Combine(path, "subtitles.srt")
            .Replace("\\", "/").Replace("C:", "C\\:");

            string arguments = @$"-y -i {"C:\\Users\\Administrador\\Desktop\\G1\\videoplayback.mp4"} -i {path + "\\GeneratedAudio.mp3"} -c:v libx264 -c:a aac -shortest {path + "\\FinalVideo.mp4"}".Replace("\r","").Replace("\n","");

            Log($"\r\n[yellow]Iniciando a geração do vídeo com os seguintes argumentos:[/] [green]{arguments}[/]");

            await videoService.GenerateVideo(arguments);

            Log("\r\n[green]Vídeo gerado e salvo com sucesso![/]");

            byte[] videoBytes = File.ReadAllBytes($@"{path}\FinalVideo.mp4");

            await mediaManager.UploadMedia(
                title: lines[0],
                description: lines[0],
                tags: lines[1].Split(",").ToList(),
                Videobytes: videoBytes
            );

            Log($"\r\n[green]Vídeo postado com sucesso![/]");
        }
        public static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Log($@"[yellow]Pasta ""{path}"" criada com sucesso![/]");
            }
            else
            {
                Log($@"[yellow]Pasta ""{path}"" já existe.[/]");
            }
        }
        public static async Task<byte[]> GenerateAudio(string text)
        {
            TextToSpeechHandler textToSpeechHandler =
                new TextToSpeechHandler(new AudioService());
            byte[] audiobytes = await textToSpeechHandler
                .Handle(new TextToSpeechCommand(text));
            return audiobytes;
        }
        public static async Task<string> GetSummarizedContent(string url)
        {
            var command = new GetSummarizedSiteContentCommand(url);

            GetSummarizedSiteContentHandler getSummarizedSiteContentHandler =
                new GetSummarizedSiteContentHandler(new AiService());

            string summarizedContent =
                await getSummarizedSiteContentHandler
                .Handle(command);

            summarizedContent = summarizedContent.Replace("*-*", "");

            return summarizedContent;
        }
        public static void Log(string message)
        {
            AnsiConsole.MarkupLine($@"|{DateTime.Now}| {message}");
        }
        public static async Task<byte[]> GenerateImage(string prompt)
        {
            GenerateImageHandler generateImageHandler =
                new GenerateImageHandler(new ImageService());
            byte[] imageBytes = await generateImageHandler
                .Handle(new GenerateImageCommand(prompt));
            return imageBytes;
        }
        //public static async Task<double> CalcSrtDuration(string text,string path)
        //{
        //    CalcSrtDurationCommand calcSrtDurationCommand =
        //        new CalcSrtDurationCommand(text,path + "\\temp_audio.wav");

        //    CalcSrtDurationHandler calcSrtDurationHandler =
        //        new CalcSrtDurationHandler(new AudioService());
        //    double duration = await calcSrtDurationHandler
        //        .Handle(calcSrtDurationCommand);

        //    return duration;
        //}
        //public static async Task<string> CreatePromptToImage(string text)
        //{
        //    CreatePromptToImageCommand createPromptToImageCommand =
        //        new CreatePromptToImageCommand(text);
        //    CreatePromptToImageHandler createPromptToImageHandler =
        //        new CreatePromptToImageHandler(new AiService());
        //    string prompt = await createPromptToImageHandler.Handle(createPromptToImageCommand);
        //    return prompt;
        //}
    }
}


