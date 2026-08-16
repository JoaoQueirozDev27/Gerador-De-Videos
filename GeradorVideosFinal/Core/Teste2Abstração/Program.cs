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

        static string[] sceneTypes = { "Image", "Image&Audio", "Video" };
        static string[] layerTypes = { "Image", "Image&Audio", "Audio", "Video" };

        static async Task<byte[]> GetImageFromUser()
        {
            string path = await AnsiConsole.Console.AskAsync<string>($"Digite o caminho da [bold yellow]imagem[/]: ");
            return File.ReadAllBytes(path);
        }

        static async Task Main(string[] args)
        {
            string BasePath = "C:\\Users\\Administrador\\Desktop\\Modelos";

            AnsiConsole.MarkupLine("[yellow]Este é seu [/] [bold yellow]Gerador de vídeos[/]");
            string option = await AnsiConsole.Console.PromptAsync(new SelectionPrompt<string>()
            .Title("Selecione uma das opções:")
            .AddChoices(new[] { "Renderizar um modelo", "Criar um modelo" }));

            if (option == "Renderizar um modelo")
            {
                string[] models = Directory.GetFiles(BasePath, "*.json");

                string modelSelected = await AnsiConsole.Console.PromptAsync(new SelectionPrompt<string>()
                .Title("Selecione uma das opções:")
                .AddChoices(models.Select(x => x.Split("\\").Last())));

                string jsonString = File.ReadAllText(Path.Combine(BasePath, modelSelected));

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

                    double sceneAudioDuration = 0;

                    switch (scene.Type)
                    {

                        case "Image":

                            byte[] imageBytes = await GetImageFromUser();

                            AnsiConsole.MarkupLine($"Imagem carregada com {imageBytes.Length} bytes.");

                            string text = scene.Subtitles == true ?
                                await AnsiConsole.Console.AskAsync<string>($"Digite o texto que será usado: ") : "";

                            if (double.TryParse(scene.Duration, out double duration) == true)
                            {
                                sceneAudioDuration = duration + 200 /*acréscimo para garantir que não vai cortar nada*/;
                            }

                            await videoService.ImageAsync(imageBytes, sceneAudioDuration, Path.Combine(BasePath, $"scene{scene.Id}.mp4"));

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
                                    sceneAudioDuration = audioService.GetAudioDuration(audioPath);
                                    AnsiConsole.MarkupLine($"Duração da cena ajustada para {sceneAudioDuration} ms com base no áudio.");
                                }
                                else if (double.TryParse(scene.Duration, out double duration2) == true)
                                {
                                    sceneAudioDuration = duration2;
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
                                sceneAudioDuration = audioService.GetAudioDuration(videoPath);
                                AnsiConsole.MarkupLine($"Duração da cena ajustada para {sceneAudioDuration} ms com base no áudio.");
                                byte[] bytes = File.ReadAllBytes(videoPath);
                                await videoService.VideoWithoutDuration(bytes, Path.Combine(BasePath, $"scene{scene.Id}.mp4"));

                            }
                            else if (double.TryParse(scene.Duration, out double duration3) == true)
                            {
                                sceneAudioDuration = duration3;
                                byte[] bytes = File.ReadAllBytes(videoPath);
                                await videoService.VideoWithDuration(bytes, sceneAudioDuration, Path.Combine(BasePath, $"scene{scene.Id}.mp4"));
                            }
                            break;
                    }

                    scenesIds.Add(scene.Id);

                    string scenePath = $"scene{scene.Id}.mp4";

                    if (scene.Layers.Count == 0)
                    {
                        // Cena sem layers: copia a cena base direto para o nome "Composed",
                        // porque a concatenação final sempre espera esse nome para TODAS as cenas.
                        File.Copy(
                            Path.Combine(BasePath, scenePath),
                            Path.Combine(BasePath, $"sceneComposed{scene.Id}.mp4"),
                            overwrite: true);
                    }
                    else
                    {
                        int layerIndex = 0;

                        foreach (var layer in scene.Layers)
                        {
                            AnsiConsole.MarkupLine($"[yellow]Layer {layer.Id}[/]");
                            AnsiConsole.MarkupLine("Tipo: " + layer.Type);
                            AnsiConsole.MarkupLine("Mix: " + (layer.Mix != null ? layer.Mix.ToString() : "Nenhum"));
                            AnsiConsole.MarkupLine("Duração: " + layer.Duration);
                            AnsiConsole.MarkupLine("Start: " + layer.Start);

                            // Nome único por CENA + LAYER, para não colidir com layers de mesmo Id de outras cenas.
                            string layerFileName = $"scene{scene.Id}_layer{layer.Id}.mp4";
                            bool layerRendered = false;

                            switch (layer.Type)
                            {
                                case "Image":
                                    {
                                        byte[] imageBytes = await GetImageFromUser();

                                        AnsiConsole.MarkupLine($"Imagem carregada com {imageBytes.Length} bytes.");

                                        string text = layer.Subtitles == true ?
                                            await AnsiConsole.Console.AskAsync<string>($"Digite o texto que será usado: ") : "";

                                        double layerDuration = 0;

                                        if (double.TryParse(layer.Duration, out double duration))
                                        {
                                            layerDuration = duration;
                                        }
                                        else
                                        {
                                            AnsiConsole.MarkupLine($"[grey]Duração '{layer.Duration}' não é numérica para layer de imagem; usando duração da cena como fallback.[/]");
                                            layerDuration = sceneAudioDuration;
                                        }

                                        await videoService.ImageAsync(imageBytes, layerDuration, Path.Combine(BasePath, layerFileName));
                                        layerRendered = true;
                                        break;
                                    }

                                case "Image&Audio":
                                    {
                                        byte[] imageBytes = await GetImageFromUser();
                                        string audioPath = "";

                                        AnsiConsole.MarkupLine($"Imagem carregada com {imageBytes.Length} bytes.");

                                        string layerAudioFile = $"scene{scene.Id}_layer{layer.Id}.wav";

                                        if (await AnsiConsole.Console.AskAsync<bool>($"Deseja carregar um áudio para a layer?") == true)
                                        {
                                            audioPath = await AnsiConsole.Console.AskAsync<string>($"Digite o caminho do [bold yellow]áudio[/]: ");
                                            AnsiConsole.MarkupLine($"Áudio {audioPath} carregado com sucesso.");
                                            layerAudioFile = audioPath;
                                        }
                                        else
                                        {
                                            string text = layer.Subtitles == true ?
                                                await AnsiConsole.Console.AskAsync<string>($"Digite o texto que será usado: ") : "";
                                            await audioService.GenerateTemporaryAudio(text, Path.Combine(BasePath, layerAudioFile));
                                            layerAudioFile = Path.Combine(BasePath, layerAudioFile);
                                        }

                                        await videoService.ImageWithAudioAsync(imageBytes, layerAudioFile, Path.Combine(BasePath, layerFileName));
                                        layerRendered = true;
                                        break;
                                    }

                                case "Video":
                                    {
                                        string videoPath = await AnsiConsole.Console.AskAsync<string>($"Digite o caminho do [bold yellow]vídeo[/]: ");
                                        AnsiConsole.MarkupLine($"Vídeo {videoPath} carregado com sucesso.");

                                        if (layer.Duration == "FromAudio")
                                        {
                                            double layerVideoDuration = audioService.GetAudioDuration(videoPath);
                                            AnsiConsole.MarkupLine($"Duração da layer ajustada para {layerVideoDuration} ms com base no áudio.");
                                            byte[] bytes = File.ReadAllBytes(videoPath);
                                            await videoService.VideoWithoutDuration(bytes, Path.Combine(BasePath, layerFileName));
                                            layerRendered = true;
                                        }
                                        else if (double.TryParse(layer.Duration, out double duration3))
                                        {
                                            byte[] bytes = File.ReadAllBytes(videoPath);
                                            await videoService.VideoWithDuration(bytes, duration3, Path.Combine(BasePath, layerFileName));
                                            layerRendered = true;
                                        }
                                        else
                                        {
                                            // "Inner": usa a duração própria do vídeo, sem cortar.
                                            byte[] bytes = File.ReadAllBytes(videoPath);
                                            await videoService.VideoWithoutDuration(bytes, Path.Combine(BasePath, layerFileName));
                                            layerRendered = true;
                                        }
                                        break;
                                    }

                                case "Audio":
                                    // Layer só de áudio não gera vídeo para sobrepor (overlay) — nada a compor visualmente.
                                    AnsiConsole.MarkupLine("[grey]Layer de áudio: sem composição visual, pulando overlay.[/]");
                                    layerRendered = false;
                                    break;
                            }

                            if (!layerRendered)
                            {
                                layerIndex++;
                                continue;
                            }

                            // Saída com nome NOVO a cada layer (nunca igual à entrada), para não ler/escrever
                            // o mesmo arquivo ao mesmo tempo no FFmpeg.
                            string composedOutput = $"sceneComposed{scene.Id}_{layerIndex}.mp4";

                            bool useOpenEndedOverlay = scene.Duration == "FromAudio" || scene.Duration == "Inner";

                            if (useOpenEndedOverlay)
                            {
                                await videoService.OverlayLayerAsync(
                                    Path.Combine(BasePath, scenePath),
                                    Path.Combine(BasePath, layerFileName),
                                    double.Parse(layer.Start),
                                    Path.Combine(BasePath, composedOutput)
                                );
                            }
                            else if (double.TryParse(scene.Duration, out double sceneDurationParsed))
                            {
                                await videoService.OverlayLayerWithDurationAsync(
                                    Path.Combine(BasePath, scenePath),
                                    Path.Combine(BasePath, layerFileName),
                                    double.Parse(layer.Start),
                                    sceneDurationParsed,
                                    Path.Combine(BasePath, composedOutput)
                                );
                            }

                            // A próxima layer usa o resultado desta composição como base.
                            scenePath = composedOutput;
                            layerIndex++;
                        }

                        // Garante que o resultado final desta cena fique salvo com o nome padrão
                        // usado depois na concatenação (sceneComposed{scene.Id}.mp4).
                        File.Copy(
                            Path.Combine(BasePath, scenePath),
                            Path.Combine(BasePath, $"sceneComposed{scene.Id}.mp4"),
                            overwrite: true);
                    }

                    AnsiConsole.MarkupLine($"[green]Processamento finalizado![/]");
                }

                await videoService.ConcatVideosAsync(scenesIds.Select(x => Path.Combine(BasePath, $"sceneComposed{x}.mp4")).ToArray(), Path.Combine(BasePath, $"videoFinal.mp4"));

                await videoService.AddMainAudioAsync(Path.Combine(BasePath, $"videoFinal.mp4"), Path.Combine(BasePath, "tempAudio.wav"), Path.Combine(BasePath, $"videoFinalWithAudio.mp4"));

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
                    .AddChoices(sceneTypes));

                    string duration = await AnsiConsole.Console.PromptAsync(new SelectionPrompt<string>()
                    .Title("Selecione uma das opções de duração: ")
                    .AddChoices(new[] { "Miliseconds", "Inner", "FromAudio" }));

                    Scene.Duration = duration == "Miliseconds" ? await AnsiConsole.Console.AskAsync<string>("Digite a duração em milissegundos: ") : duration;

                    Scene.Subtitles = await AnsiConsole.Console.ConfirmAsync("Deseja ativar legendas?");

                    if (await AnsiConsole.Console.ConfirmAsync("Deseja usar IA para gerar o conteúdo(áudio) desta cena?") == true)
                    {
                        Scene.Prompt = await AnsiConsole.Console.AskAsync<string>(
                            "Digite o prompt da cena (use /*variavel*/ para partes dinâmicas, a palavra 'variavel' pode ser trocada para o nome que desejar): ");
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
                            .AddChoices(layerTypes));

                            layer.Mix = await AnsiConsole.Console.AskAsync<int>("Digite o mix (1: toca junto com o main, 0: pausa o main): ");

                            string LayerDuration = await AnsiConsole.Console.PromptAsync(new SelectionPrompt<string>()
                            .Title("Selecione uma das opções de duração: ")
                            .AddChoices(new[] { "Miliseconds", "Inner", "FromAudio" }));

                            // CORRIGIDO: antes usava a variável "duration" (da cena) por engano.
                            // Agora usa "LayerDuration", que é a escolha feita para ESTA layer.
                            layer.Duration = LayerDuration == "Miliseconds" ? await AnsiConsole.Console.AskAsync<string>("Digite a duração em milissegundos: ") : LayerDuration;

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
                File.WriteAllText(Path.Combine(BasePath, $"{videoProject.TemplateName}.json"), jsonString);
            }
        }
    }
}