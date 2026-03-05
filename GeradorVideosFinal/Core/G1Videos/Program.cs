using Application.interfaces;
using Application.UseCases.Audio.TextToSpeech;
using Application.UseCases.IA.GetSummarizedSiteContentCommand;
using Application.UseCases.Imagem.GenerateImage;
using Domain.Entities;
using Services;
using Services.Factories;
using Spectre.Console;

namespace Presentation
{
    class Program
    {
        static IAiService aiService = new AiService();
        static IAudioService audioService = new AudioService();
        static IVideoService videoService = new VideoService();
        static IMediaManager mediaManager = YoutubeManagerFactory.CreateYoutubeManagerInstance();
        static WebScrappingService webScrappingService = new WebScrappingService();

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

        public static void Log(string message)
        {
            AnsiConsole.MarkupLine($@"|{DateTime.Now}| {message}");
        }

        static async Task Main(string[] args)
        {
            Console.InputEncoding = System.Text.Encoding.UTF8;
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            AnsiConsole.Write(new FigletText("Video Creator").Centered().Color(Spectre.Console.Color.Green));

            Log("[yellow]Iniciando o processo de criação de vídeos...[/]");

            CreateDirectoryIfNotExists("C:\\Users\\Administrador\\Desktop\\G1");

            RunModel((await webScrappingService.GetNews("")).Select(x => x.content).ToList(),"");
        }

        static async void RunModel(List<string> contents,string prompt)
        {
            for (int i = 0; i < contents.Count; i++)
            {
                await ProcessVideo(contents[i], i,prompt);
            }
        }

        static async Task ProcessVideo(string content,int i,string prompt)
        {
            string path = $"C:\\Users\\Administrador\\Desktop\\G1\\Video{i}".Replace("\r", "").Replace("\n", "");

            CreateDirectoryIfNotExists(path);

            List<string> lines = new List<string>();

            string response = await aiService.TransformText(prompt,content);

            var responseJson = System.Text.Json.JsonDocument.Parse(response);

            string? responseContent = responseJson.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

            Log($"\r\n[green]Roteiro gerado com sucesso![/]");
            Log($"\r\n[yellow]{responseContent}[/]");

            lines = responseContent.Replace("\n\n", "\n").Split("~").ToList();

            if (lines.Count == 0)
            {
                Log($"\r\n[red]O roteiro gerado não está no formato esperado. Tentando novamente...[/]");
                return;
            }

            Log($"\r\n[yellow]Título do vídeo: [/][green]{lines[0]}[/]");

            Log($"\r\n[yellow]Conteúdo do vídeo: [/][green]{string.Join(" ", lines[2..])}[/]");

            File.WriteAllText($"{path}\\SummarizedContent.txt", string.Join(" ", lines[2..]));

            await audioService.GenerateTemporaryAudio(string.Join(" ", lines[2..]), path + "\\GeneratedAudio.wav");

            await videoService
                .GenerateVideo(@$"-y -i {path + "\\GeneratedAudio.wav"} -filter:a loudnorm -codec:a libmp3lame -qscale:a 2 {path + "\\GeneratedAudio.mp3"}");

            string arguments = $@"-y -i ""C:\Users\Administrador\Desktop\G1\videoplayback.mp4"" -i ""{path}\GeneratedAudio.mp3"" -c:v copy -c:a aac -shortest -movflags +faststart ""{path}\FinalVideo.mp4""";

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
    }
}


