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

namespace Presentation {
    class Program {

        static IAiService aiService = new AiService();
        static IAudioService audioService = new AudioService();
        static IVideoService videoService = new VideoService();
        static IMediaManager mediaManager = YoutubeManagerFactory.CreateYoutubeManagerInstance();
        static async Task Main(string[] args)
        {
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            AnsiConsole.Write(new FigletText("Video Creator").Centered().Color(Color.Green));

            Log("[yellow]Iniciando o processo de criação de vídeos...[/]");

            var temasEngracados = new List<string>
            {
                "Primeiro encontro que deu errado",
                "Entrevista de emprego que virou trauma",
                "Tentando cozinhar sem saber fritar um ovo",
                "Primeiro dia na academia",
                "O dia em que o Wi-Fi caiu e eu descobri o que é o tédio",
                "Trabalhar de home office com a família em casa",
                "Compra online que parecia perfeita, mas veio de Nárnia",
                "Briga com tecnologia",
                "Aquele dia que você tentou ser adulto e falhou miseravelmente",
                "História de amor no transporte público",
                "Tentando parecer inteligente em reunião e sendo desmentido pelo Excel",
                "Meu pet sabotando minha dignidade",
                "Fazendo dieta e sendo traído pelo próprio estômago",
                "Quando o GPS decidiu me testar psicologicamente",
                "Tentando consertar algo 'simples' e piorando 300%",
                "O dia em que mandei mensagem pra pessoa errada",
                "A vez que tentei parecer calmo e tropecei na minha própria sombra",
                "História de terror: minha conta bancária no fim do mês",
                "Tentando flertar e soando igual um bot de atendimento",
                "Quando você tenta ser produtivo e o universo conspira por um cochilo"
            };

            CreateDirectoryIfNotExists("C:\\Users\\Administrador\\Desktop\\Videos");

            #region Processing Video in Paralell

            List<string[]> conjuntos = temasEngracados.Chunk(1).ToList();

            //Directory.Delete("C:\\Users\\Administrador\\Desktop\\Videos",true);

            foreach (string[] conjunto in conjuntos)
            {
                List<Task> tasks = new List<Task>();

                foreach (var tema in conjunto)
                {
                    tasks.Add(ProcessVideo(tema));
                };
                await Task.WhenAll(tasks);
            };
            #endregion
        }
        public static async Task ProcessVideo(string tema)
        {
            //Log($"\r\n[yellow]Processando o tema:[/] [green]{tema}[/]");
            
            string path = $@"C:\Users\Administrador\Desktop\Videos\{tema.Replace(" ", "_").Replace(":", "")}";

            CreateDirectoryIfNotExists(path);

            string summarizedContent = await aiService.GetRoadMap(tema);

            Log("\r\n\r\n[green]Conteúdo resumido recebido com sucesso![/]");

            var responseJson = System.Text.Json.JsonDocument.Parse(summarizedContent);

            string responseContent = responseJson.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

            string[] lines = responseContent.Split("~");

            string ContentToSave = string.Join("",lines[1..]);

            File.WriteAllText($"{path}\\SummarizedContent.txt",ContentToSave.Replace("~", " "));

            Log("\r\n\r\n[green]Conteúdo resumido salvo com sucesso![/]");

            /* Gera audio com ElevenLabs */

            //byte[] audioBytes = await GenerateAudio(responseContent.Replace("~", ""));
            //File.WriteAllBytes($"{path}\\GeneratedAudio.mp3", audioBytes);

            /*Gera local,apenas para testes*/

            //ARRUMAR ESSA LINHA DO AUDIO

            await audioService.GenerateTemporaryAudio(ContentToSave, "C:\\KokoroModel\\kokoro.onnx", path + "\\GeneratedAudio.wav");

            Log("\r\n[green]Áudio gerado e salvo com sucesso![/]");

            #region SRT Generation
            //int j = 0;
            //TimeSpan tempoInicial = TimeSpan.FromSeconds(0);
            //string srtContent = "";
            //foreach (var line in responseContent.Split("*-*"))
            //{
            //    srtContent += $"{j + 1}\r\n\r\n";
            //    double duration = await CalcSrtDuration(line);
            //    TimeSpan tempoFinal = tempoInicial.Add(TimeSpan.FromSeconds(duration));
            //    srtContent += $"{tempoInicial:hh\\:mm\\:ss\\,fff} --> {tempoFinal:hh\\:mm\\:ss\\,fff}\r\n\r\n";
            //    srtContent += line.Trim();
            //    srtContent += "\r\n\r\n";
            //    tempoInicial = tempoFinal;
            //    j++;
            //}
            #endregion

            await videoService
                .GenerateVideo(@$"-y -i {path + "\\GeneratedAudio.wav"} -filter:a loudnorm -codec:a libmp3lame -qscale:a 2 {path + "\\GeneratedAudio.mp3"}");

            string arguments = @$"-y -i {"C:\\Users\\Administrador\\Desktop\\Videos\\videoplayback.mp4"} -i {path + "\\GeneratedAudio.mp3"} -c:v libx264 -c:a aac -shortest {path + "\\FinalVideo.mp4"}".Replace("\r", "").Replace("\n", "");

            await videoService.GenerateVideo(arguments);
            Log("\r\n[green]Vídeo gerado e salvo com sucesso![/]");

            Log($"\r\n[yellow]Postando o vídeo do tema:[/] [green]{tema}[/]");

            Log($"\r\n[yellow]Título do vídeo: [/][green]{lines[0]}[/]");

            byte[] videoBytes = File.ReadAllBytes($"{path}\\FinalVideo.mp4");

            await mediaManager.UploadMedia(
                title: lines[0],
                description: $"Vídeo engraçado sobre: {tema}",
                tags: new List<string> { "engraçado", "comédia", "humor", "curto", "vídeo curto" },
                Videobytes: videoBytes);

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
        public static async Task<double> CalcSrtDuration(string text,string model,string path)
        {
            CalcSrtDurationCommand calcSrtDurationCommand =
                new CalcSrtDurationCommand(text,model,path);

            CalcSrtDurationHandler calcSrtDurationHandler =
                new CalcSrtDurationHandler(new AudioService());
            double duration = await calcSrtDurationHandler
                .Handle(calcSrtDurationCommand);

            return duration;
        }
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

