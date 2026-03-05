using Application.interfaces;
using Services;
using Services.Factories;
using Spectre.Console;
using System;
using System.IO;
using Domain.Entities;
using NAudio.Utils;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection.Emit;

namespace Presentation
{   
    
    class Program
    {
        static IAiService aiService = new AiService();
        static IAudioService audioService = new AudioService();
        static VideoService videoService = new VideoService();
        
        static WebScrappingService webScrappingService = new WebScrappingService();

        static string[] types = { "Image", "Image&Audio", "Audio", "Video" };
                        
        static async Task<byte[]> GetImageFromUser()
        {
            string path = await AnsiConsole.Console.AskAsync<string>($"Digite o caminho da [bold yellow]imagem[/]: ");
            return File.ReadAllBytes(path);
        } 

        static async Task Main(string[] args)
        {
            //mediaManager = await YoutubeManagerFactory.CreateYoutubeManagerInstance();

            string BasePath = "C:\\Users\\Administrador\\Desktop\\Modelos";

            AnsiConsole.MarkupLine("[yellow]Este é seu [/] [bold yellow]Gerador de vídeos[/]");
            string option = await AnsiConsole.Console.PromptAsync(new SelectionPrompt<string>()
            .Title("Selecione uma das opções:")
            .AddChoices(new[] { "Renderizar um modelo", "Criar um modelo"}));

            if (option == "Renderizar um modelo")
            {   
                string[] models = Directory.GetFiles(BasePath,"*.json");

                string modelSelected = await AnsiConsole.Console.PromptAsync(new SelectionPrompt<string>()
                .Title("Selecione uma das opções:")
                .AddChoices(models.Select(x => x.Split("\\").Last())));
                
                string jsonString = File.ReadAllText(Path.Combine(BasePath,modelSelected));

                VideoProject videoProject = System.Text.Json.JsonSerializer.Deserialize<VideoProject>(jsonString);
                
                AnsiConsole.MarkupLine($"[green]Modelo {videoProject.TemplateName} carregado com sucesso![/]");
                AnsiConsole.MarkupLine($"[green]Iniciando processamento...[/]");
                AnsiConsole.MarkupLine($"[green]=========================================================[/]");
                AnsiConsole.MarkupLine("ID: " + videoProject.Id);
                AnsiConsole.MarkupLine("Resolução: " + videoProject.Resolution);
                AnsiConsole.MarkupLine("Áudio principal: " + (videoProject.AudioConfig == true ? "Ativado" : "Desativado"));

                AnsiConsole.MarkupLine("Prompt do áudio principal: " + (videoProject.Prompt != null ? videoProject.Prompt : "Nenhum"));

                if (videoProject.Prompt.Contains("/*") & videoProject.Prompt.Contains("*/"))
                {
                    string varName = videoProject.Prompt.Split("/*")[1].Split("*/")[0];
                    string userInput = await AnsiConsole.Console.AskAsync<string>($"Digite o valor para a variável [bold]{varName}[/]: ");
                    videoProject.Prompt = videoProject.Prompt.Replace($"/*{varName}*/", userInput);
                }

                string videoContent = await aiService.SendPrompt(videoProject.Prompt);

                AnsiConsole.MarkupLine("[green]Conteúdo gerado para o vídeo principal:[/]\n" + videoContent);

                AnsiConsole.MarkupLine("[green] Gerando áudio(via TTS)...[/]");

                await audioService.GenerateTemporaryAudio(videoContent, Path.Combine(BasePath, "tempAudio.wav"));

                List<int> scenesIds = new List<int>();

                foreach (var scene in videoProject.Scenes)
                {
                    AnsiConsole.MarkupLine($"[blue]Cena {scene.Id}[/]");
                    AnsiConsole.MarkupLine("Tipo: " + scene.Type);
                    AnsiConsole.MarkupLine("Duração: " + scene.Duration);
                    AnsiConsole.MarkupLine("Legendas: " + (scene.Subtitles == true ? "Ativado" : "Desativado"));
                    AnsiConsole.MarkupLine("Prompt: " + (scene.Prompt != null ? scene.Prompt : "Nenhum"));

                    List<int> layersIds = new List<int>();
                    //await videoService.ConcatVideosAsync(layersIds.Select(x => Path.Combine(BasePath, $"layer{x}.mp4")).ToArray(), Path.Combine(BasePath, $"scene{scene.Id}.mp4"));

                    switch (scene.Type)
                    {
                        case "Image":

                            byte[] imageBytes = await GetImageFromUser();

                            AnsiConsole.MarkupLine($"Imagem carregada com {imageBytes.Length} bytes.");

                            string text = scene.Subtitles == true ?
                                await AnsiConsole.Console.AskAsync<string>($"Digite o texto que será usado: ") : "";

                            double audioDuration = 0;

                            if (double.TryParse(scene.Duration, out double duration) == true)
                            {
                                audioDuration = duration + 200 /*acréscimo para garantir que não vai cortar nada*/;
                            }

                            await videoService.ImageAsync(imageBytes, audioDuration, Path.Combine(BasePath, $"scene{scene.Id}.mp4"));

                            break;

                        case "Image&Audio":
                            imageBytes = await GetImageFromUser();
                            string audioPath = "";
                            AnsiConsole.MarkupLine($"Imagem carregada com {imageBytes.Length} bytes.");
                            if (await AnsiConsole.Console.AskAsync<bool>($"Deseja carregar um áudio para a cena?") == true)
                            {
                                audioPath = await AnsiConsole.Console.AskAsync<string>($"Digite o caminho do [bold yellow]áudio[/]: ");
                                AnsiConsole.MarkupLine($"Áudio {audioPath} carregado com sucesso.");

                                if (scene.Duration == "FromAudio")
                                {
                                    audioDuration = audioService.GetAudioDuration(audioPath);
                                    AnsiConsole.MarkupLine($"Duração da cena ajustada para {audioDuration} ms com base no áudio.");
                                }
                                else if (double.TryParse(scene.Duration, out double duration2) == true)
                                {
                                    audioDuration = duration2;
                                }
                            }
                            else
                            {
                                text = scene.Subtitles == true ?
                                await AnsiConsole.Console.AskAsync<string>($"Digite o texto que será usado: ") : "";
                                await audioService.GenerateTemporaryAudio(text, Path.Combine(BasePath, $"scene{scene.Id}.wav"));
                            }

                            await videoService.ImageWithAudioAsync(imageBytes, Path.Combine(BasePath, $"scene{scene.Id}.wav"), Path.Combine(BasePath, $"scene{scene.Id}.mp4"));

                            break;

                        case "Video":
                            string videoPath = await AnsiConsole.Console.AskAsync<string>($"Digite o caminho do [bold yellow]vídeo[/]: ");
                            AnsiConsole.MarkupLine($"Vídeo {videoPath} carregado com sucesso.");
                            if (scene.Duration == "FromAudio")
                            {
                                audioDuration = audioService.GetAudioDuration(videoPath);
                                AnsiConsole.MarkupLine($"Duração da cena ajustada para {audioDuration} ms com base no áudio.");
                                byte[] bytes = File.ReadAllBytes(videoPath);
                                await videoService.VideoWithoutDuration(bytes, Path.Combine(BasePath, $"scene{scene.Id}.mp4"));

                            }
                            else if (double.TryParse(scene.Duration, out double duration3) == true)
                            {
                                audioDuration = duration3;
                                byte[] bytes = File.ReadAllBytes(videoPath);
                                await videoService.VideoWithDuration(bytes, audioDuration, Path.Combine(BasePath, $"scene{scene.Id}.mp4"));
                            }
                            break;
                    }
                    
                    scenesIds.Add(scene.Id);

                    foreach (var layer in scene.Layers)
                    {
                        AnsiConsole.MarkupLine($"[yellow]Layer {layer.Id}[/]");
                        AnsiConsole.MarkupLine("Tipo: " + layer.Type);
                        AnsiConsole.MarkupLine("Mix: " + (layer.Mix != null ? layer.Mix.ToString() : "Nenhum"));
                        AnsiConsole.MarkupLine("Duração: " + layer.Duration);
                        AnsiConsole.MarkupLine("Start: " + layer.Start);

                        switch (layer.Type)
                        {
                            case "Image":

                                byte[] imageBytes = await GetImageFromUser();

                                AnsiConsole.MarkupLine($"Imagem carregada com {imageBytes.Length} bytes.");

                                string text = layer.Subtitles == true ?
                                    await AnsiConsole.Console.AskAsync<string>($"Digite o texto que será usado: ") : "";

                                double audioDuration = 0;

                                //if (layer.Duration == "FromAudio")
                                //{
                                //    await audioService.GenerateTemporaryAudio(text, Path.Combine(BasePath, $"layer{layer.Id}.wav"));
                                //    audioDuration = videoService.GetDurationMs(Path.Combine(BasePath, $"layer{layer.Id}.wav"));
                                //    AnsiConsole.MarkupLine($"Duração da layer ajustada para {audioDuration} ms com base no áudio.");
                                //}
                                if (double.TryParse(layer.Duration, out double duration) == true)
                                {
                                    audioDuration = duration;
                                }
                                else
                                {
                                    break;
                                }

                                await videoService.ImageAsync(imageBytes, audioDuration, Path.Combine(BasePath, $"layer{layer.Id}.mp4"));

                                break;

                            case "Image&Audio":

                                imageBytes = await GetImageFromUser();

                                string audioPath = "";

                                AnsiConsole.MarkupLine($"Imagem carregada com {imageBytes.Length} bytes.");

                                if (await AnsiConsole.Console.AskAsync<bool>($"Deseja carregar um áudio para a layer?") == true)
                                {
                                    audioPath = await AnsiConsole.Console.AskAsync<string>($"Digite o caminho do [bold yellow]áudio[/]: ");
                                    AnsiConsole.MarkupLine($"Áudio {audioPath} carregado com sucesso.");

                                    if (layer.Duration == "FromAudio")
                                    {
                                        audioDuration = videoService.GetDurationMs(audioPath);
                                        AnsiConsole.MarkupLine($"Duração da layer ajustada para {audioDuration} ms com base no áudio.");
                                    }
                                    else if (double.TryParse(layer.Duration, out double duration2) == true)
                                    {
                                        audioDuration = duration2;
                                    }
                                }
                                else
                                {
                                    text = layer.Subtitles == true ?
                                    await AnsiConsole.Console.AskAsync<string>($"Digite o texto que será usado: ") : "";
                                    await audioService.GenerateTemporaryAudio(text, Path.Combine(BasePath, $"layer{layer.Id}.wav"));
                                }

                                await videoService.ImageWithAudioAsync(imageBytes, Path.Combine(BasePath, $"layer{layer.Id}.wav"), Path.Combine(BasePath, $"layer{layer.Id}.mp4"));

                                break;

                            case "Video":

                                string videoPath = await AnsiConsole.Console.AskAsync<string>($"Digite o caminho do [bold yellow]vídeo[/]: ");

                                AnsiConsole.MarkupLine($"Vídeo {videoPath} carregado com sucesso.");

                                if (layer.Duration == "FromAudio")
                                {
                                    audioDuration = audioService.GetAudioDuration(videoPath);
                                    AnsiConsole.MarkupLine($"Duração da layer ajustada para {audioDuration} ms com base no áudio.");
                                    byte[] bytes = File.ReadAllBytes(videoPath);
                                    await videoService.VideoWithoutDuration(bytes, Path.Combine(BasePath, $"layer{layer.Id}.mp4"));
                                }
                                else if (double.TryParse(layer.Duration, out double duration3) == true)
                                {
                                    audioDuration = duration3;

                                    byte[] bytes = File.ReadAllBytes(videoPath);

                                    await videoService.VideoWithDuration(bytes, audioDuration, Path.Combine(BasePath, $"layer{layer.Id}.mp4"));
                                }

                                break;

                                /*
                                 *Soma as layers primeiro, forma a cena, depois soma as cenas para formar o vídeo final
                                 *essa é a lógica básica que deve ser implementada
                                 *monta a soma das layers usando as caracteristicas do scene como regra
                                 *soma as cenas sob as regra do video project
                                 *faz várias concatenações
                                 *NAO QUEIRA FAZER UM FFMPEG GIGANTE   
                                 *faz os pequenos e concatena e vai subindo
                                 *nao inventa moda
                                 *se fizer isso, talvez voce acabe amanhã com um video pronto
                                 *e depois é só rodar.
                                 *boa sorte!
                                 */
                        }

                        //USA O OVERLAY PARA COLOCAR AS LAYERS

                        if (scene.Duration == "FromAudio")
                        {
                            //double sceneDuration = audioService.GetAudioDuration(Path.Combine(BasePath, $"scene{scene.Id}.wav"));
                            //AnsiConsole.MarkupLine($"Duração da cena ajustada para {sceneDuration} ms com base no áudio.");

                            await videoService.
                            OverlayLayerAsync(
                                Path.Combine(BasePath, $"scene{scene.Id}.mp4"),
                                Path.Combine(BasePath, $"layer{layer.Id}.mp4"),
                                double.Parse(layer.Start),
                                Path.Combine(BasePath, $"scene{layer.Id}.mp4")
                            );
                        }
                        else if (double.TryParse(scene.Duration, out double sceneDuration) == true)
                        {
                            await videoService.
                            OverlayLayerWithDurationAsync(
                                Path.Combine(BasePath, $"scene{scene.Id}.mp4"),
                                Path.Combine(BasePath, $"layer{layer.Id}.mp4"),
                                double.Parse(layer.Start),
                                sceneDuration,
                                Path.Combine(BasePath, $"scene{layer.Id}.mp4")
                            );
                        }
                    }

                    AnsiConsole.MarkupLine($"[green]Processamento finalizado![/]");
                }

                await videoService.ConcatVideosAsync(scenesIds.Select(x => Path.Combine(BasePath, $"scene{x}.mp4")).ToArray(), Path.Combine(BasePath, $"videoFinal.mp4"));

            }
            else if (option == "Criar um modelo")
            {
                VideoProject videoProject = new VideoProject();
                videoProject.Id = Guid.NewGuid();
                videoProject.TemplateName = await AnsiConsole.Console.AskAsync<string>("Digite o nome do modelo: ");
                videoProject.Resolution = await AnsiConsole.Console.PromptAsync(new SelectionPrompt<string>()
                .Title("Selecione uma das resoluções: ")
                .AddChoices(new[] { "9:16", "16:9" }));

                videoProject.AudioConfig = await AnsiConsole.Console.ConfirmAsync("Deseja ativar o áudio principal?");

                if (videoProject.AudioConfig == true)
                {
                    if (await AnsiConsole.Console.ConfirmAsync("Deseja usar IA para geração do texto que será usado para criação do áudio?") == true)
                        videoProject.Prompt =
                            await AnsiConsole
                            .Console
                            .AskAsync<string>("Digite o prompt para geração do áudio principal,caso vá usar alguma parte do prompt que possa variar(texto base, por exemplo), use /*espaço para texto variante*/, coloque o nome que desejar: ");
                    else
                        videoProject.Prompt = null;
                }

                bool addScene = true;
                int sceneId = 1;
                do
                {           
                    AnsiConsole.MarkupLine($"[green]Adicionando cena {sceneId}[/]");
                    Scene Scene = new Scene();
                    Scene.Id = sceneId;

                    Scene.Type = await AnsiConsole.Console.PromptAsync(new SelectionPrompt<string>()
                    .Title("Selecione uma das opções de tipo: ")
                    .AddChoices(types));

                    string duration = await AnsiConsole.Console.PromptAsync(new SelectionPrompt<string>()
                    .Title("Selecione uma das opções de duração: ")
                    .AddChoices(new[] { "Miliseconds", "Inner", "FromAudio"}));

                    Scene.Duration = duration == "Miliseconds" ? await AnsiConsole.Console.AskAsync<string>("Digite a duração em milissegundos: ") : duration;

                    //Scene.Text = await AnsiConsole.Console.AskAsync<string>("Digite o texto da cena(use ~ em volta do texto que nao deve virar audio): ");

                    Scene.Subtitles = await AnsiConsole.Console.ConfirmAsync("Deseja ativar legendas?");

                    if (await AnsiConsole.Console.ConfirmAsync("Deseja usar IA para gerar o conteúdo(áudio) desta cena?") == true)
                    {
                        Scene.Prompt = await AnsiConsole.Console.AskAsync<string>(
                            "Digite o prompt da cena (use /*variavel*/ para partes dinâmicas): ");
                    }
                    else
                    {
                        Scene.Prompt = null;
                    }

                    bool CreateLayer = await AnsiConsole.Console.ConfirmAsync("Criar layers?");

                    if (CreateLayer == true)
                    {
                        bool addLayer = true;
                        int layerId = 0;
                        do
                        {
                            Layer layer = new Layer();

                            layer.Id = layerId;

                            layer.Type = await AnsiConsole.Console.PromptAsync(new SelectionPrompt<string>()
                            .Title("Selecione uma das opções de tipo: ")
                            .AddChoices(types));

                            layer.Mix = await AnsiConsole.Console.AskAsync<int?>("Digite o mix (1: toca junto com o main, 0: pausa o main): ");

                            string LayerDuration = await AnsiConsole.Console.PromptAsync(new SelectionPrompt<string>()
                            .Title("Selecione uma das opções de duração: ")
                            .AddChoices(new[] { "Miliseconds", "Inner", "FromAudio" }));

                            layer.Duration = LayerDuration == "Miliseconds" ? await AnsiConsole.Console.AskAsync<string>("Digite a duração em milissegundos: ") : duration;

                            layer.Start = await AnsiConsole.Console.AskAsync<string>("Digite o start (relativo ao início da cena): ");

                            Scene.Layers.Add(layer);

                            layerId++;

                            addLayer = await AnsiConsole.Console.PromptAsync(new ConfirmationPrompt("Deseja adicionar mais uma layer?"));
                        }
                        while (addLayer == true);
                    }

                    videoProject.Scenes.Add(Scene);

                    addScene = await AnsiConsole.Console.PromptAsync(new ConfirmationPrompt("Deseja adicionar mais uma cena?"));
                    sceneId++;
                }
                while (addScene == true);

                

                var jsonString = System.Text.Json.JsonSerializer.Serialize(videoProject, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText( Path.Combine(BasePath,$"{videoProject.TemplateName}.json"), jsonString);
            }
        }
    }
}


