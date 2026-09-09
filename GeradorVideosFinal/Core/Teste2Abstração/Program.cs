using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Services.ContentSources.AiContentSource;
using Services.ContentSources.GoogleImageContent;
using Services.ContentSources.RssContentSource;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Reflection;
using System.Text.RegularExpressions;
using Services;

namespace Presentation
{
    class Program
    {

        static string[] sceneTypes = { "Image", "Image&Audio", "Video" };
        static string[] layerTypes = { "Image", "Image&Audio", "Audio", "Video" };

        static async Task<byte[]> GetImageFromUser()
        {
            string path = await AnsiConsole.Console.AskAsync<string>($"Digite o caminho da [bold yellow]imagem[/]: ");
            return File.ReadAllBytes(path);
        }

        static async Task Main(string[] args)
        {

            Spectre.Console.AnsiConsole.Console.Write(new FigletText("Video").Color(Color.Yellow).Centered());
            Spectre.Console.AnsiConsole.Console.Write(new FigletText("Generator").Color(Color.Yellow).Centered());
            string BasePath = "C:\\Users\\Administrador\\Desktop\\Modelos";

            AnsiConsole.MarkupLine("[yellow]Este é seu [/] [bold yellow]Gerador de vídeos[/]");

            bool sair = false;

            while (!sair)
            {
                string option = await SelectOption(new[] { "Criar um modelo estático(ME)", "Criar um modelo dinâmico(MD)", "Sair" });

                VideoProject? videoProject = new VideoProject();

                var jsonString = "";

                switch (option)
                {
                    case "Sair":
                        sair = true;
                        break;
                    /*
                    case "Renderizar um modelo estático":
                        string[] models = Directory.GetFiles(BasePath, "*.json");

                        string modelSelected = await AnsiConsole.Console.PromptAsync(new SelectionPrompt<string>()
                        .Title("Selecione uma das opções:")
                        .AddChoices(models.Select(x => x.Split("\\").Last())));

                        jsonString = File.ReadAllText(Path.Combine(BasePath, modelSelected));

                        videoProject = System.Text.Json.JsonSerializer.Deserialize<VideoProject>(jsonString);

                        AnsiConsole.MarkupLine($"[green]Modelo {videoProject.TemplateName} carregado com sucesso![/]");
                        AnsiConsole.MarkupLine($"[green]Iniciando processamento...[/]");
                        AnsiConsole.MarkupLine($"[green]=========================================================[/]");
                        AnsiConsole.MarkupLine("ID: " + videoProject.Id);
                        AnsiConsole.MarkupLine("Resolução: " + videoProject.Resolution);
                        AnsiConsole.MarkupLine("Áudio principal: " + (videoProject.AudioConfig == true ? "Ativado" : "Desativado"));

                        AnsiConsole.MarkupLine("Prompt do áudio principal: " + (videoProject.Prompt != null ? videoProject.Prompt : "Nenhum"));

                        if (videoProject.Prompt.Contains("/*") & videoProject.Prompt.Contains(""))
                        {
                            string varName = videoProject.Prompt.Split("/*")[1].Split("")[0];
                            string userInput = await AnsiConsole.Console.AskAsync<string>($"Digite o valor para a variável [bold]{varName}[/]: ");
                            videoProject.Prompt = videoProject.Prompt.Replace($"/*{varName}", userInput);
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
                                        sceneAudioDuration = duration + 200 /*acréscimo para garantir que não vai cortar nada;
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

                        break;
                */

                    case "Criar um modelo estático(ME)":
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

                        jsonString = System.Text.Json.JsonSerializer.Serialize(videoProject, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                        File.WriteAllText(Path.Combine(BasePath, $"{videoProject.TemplateName}.json"), jsonString);
                        break;

                    case "Criar um modelo dinâmico(MD)":
                        string[] models = Directory.GetFiles(BasePath, "*.json");

                        string modelSelected = await SelectOption(models.Select(x => x.Split("\\").Last()).ToList());

                        jsonString = File.ReadAllText(Path.Combine(BasePath, modelSelected));

                        videoProject = System.Text.Json.JsonSerializer.Deserialize<VideoProject>(jsonString);

                        if (videoProject == null)
                        {
                            AnsiConsole.MarkupLine($"[red]Erro ao carregar o modelo {modelSelected}[/]");
                            continue;
                        }

                        AnsiConsole.MarkupLine($"[green]Modelo {videoProject.TemplateName} carregado com sucesso![/]");

                        ExecutableModel ExecutableModel = new ExecutableModel(videoProject);

                        ExecutableModel.Name = await AnsiConsole.Console.AskAsync<string>("Digite um nome para seu [yellow]modelo dinâmico[/]: ");

                        List<IContentSource> ContentSources = [
                            new AiContentSource(),
                            new G1RssContentSource(),
                            new GoogleImageContentSource()
                        ];

                        AnsiConsole.MarkupLine($"escolha uma das fontes de conteúdo para a [green]Origem[/]:");

                        (IContentSource ContentSource, object? Request, object? Response) SelectedContentSource = await SelectContentSource(ContentSources);

                        ExecutableModel.AddContentSource(new ContentSourceKey(0, 0, 0), SelectedContentSource);

                        Text conteudo = new Text(
                            string.Join("\n", SelectedContentSource.ContentSource.ContentType.GetProperties().Select(p => p.Name))
                        ).Centered();

                        IRenderable painel = new Panel(conteudo)
                        {
                            Header = new PanelHeader("Para usar o conteúdo da origem, siga o padrão /*origin.property*/, substitua propertie por uma das propriedades abaixo:"), 
                            Border = BoxBorder.Rounded                   
                        };

                        AnsiConsole.Console.Write(painel);

                        if (ExecutableModel.Project.hasMainAudio())
                        {
                            AnsiConsole.MarkupLine($"escolha uma das fontes de conteúdo para o [green]áudio principal[/]:");
                            ExecutableModel.AddContentSource(new ContentSourceKey(1, 0, 0), await SelectContentSource(ContentSources));
                        }

                        int count = 0;
                        
                        foreach (Scene scene in ExecutableModel.Project.Scenes)
                        {
                            count++;
                            if (scene == null)
                            {
                                AnsiConsole.MarkupLine($"Cena {count} está registrada, mas não existe");
                                continue;
                            }
                            AnsiConsole.MarkupLine($"[yellow]Cena {count}.[/]");
                            AnsiConsole.MarkupLine($"[yellow]Tipo {count}.[/]");
                            ExecutableModel.AddContentSource(new ContentSourceKey(null, scene.Id, null), await SelectContentSource(ContentSources));
                            int countLayer = 0;
                            foreach (Layer layer in scene.Layers)
                            {
                                if (layer == null)
                                {
                                    AnsiConsole.MarkupLine($"Camada {countLayer} está registrada, mas não existe");
                                    continue;
                                }
                                AnsiConsole.MarkupLine($"[yellow]Camada {count}[/]");
                                ExecutableModel.AddContentSource(new ContentSourceKey(null, scene.Id, layer.Id), await SelectContentSource(ContentSources));
                            }
                        }

                        /*
                            jsonString = System.Text.Json.JsonSerializer.Serialize(ExecutableModel, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                            File.WriteAllText(Path.Combine(BasePath, "Dinamicos", $"{ExecutableModel.Name}.json"), jsonString);
                        */

                        /*  O modelo dinâmico é salvo em uma pasta separada chamada "Dinamicos" para diferenciar dos modelos estáticos.*/
                        /*  O código devia parar aqui, mas como a persistencia em JSON está ruim, vou continuar aqui a execução, depois separo.*/

                        /*A PARTIR DAQUI COMECA A RENDERIZAR*/

                        VideoService videoService = new VideoService();
                        AudioService audioService = new AudioService();
                        ImageService imageService = new ImageService();

                        string dinamicosPath = Path.Combine(BasePath, "Dinamicos");

                        Directory.CreateDirectory(dinamicosPath);

                        string renderedVideoPath = await RenderDynamicModel(
                            ExecutableModel,
                            videoService,
                            audioService,
                            imageService,
                            BasePath,
                            dinamicosPath
                        );

                        AnsiConsole.MarkupLine(
                            $"[green]Modelo dinâmico renderizado com sucesso![/]"
                        );

                        AnsiConsole.MarkupLine(
                            $"[green]Vídeo salvo em:[/] {renderedVideoPath}"
                        );


                        break;
                }
            }
        }

        private static async Task<string> RenderDynamicModel(
    ExecutableModel executableModel,
    VideoService videoService,
    AudioService audioService,
    ImageService imageService,
    string basePath,
    string dinamicosPath)
        {
            if (executableModel == null)
                throw new ArgumentNullException(nameof(executableModel));

            if (executableModel.Project == null)
                throw new ArgumentException(
                    "ExecutableModel não possui um VideoProject."
                );

            if (executableModel.ContentSources == null)
                throw new ArgumentException(
                    "O modelo dinâmico não possui ContentSources."
                );

            if (executableModel.Project.Scenes == null ||
                executableModel.Project.Scenes.Count == 0)
                throw new ArgumentException(
                    "O modelo não possui cenas para renderizar."
                );

            /*
             * Antes de renderizar qualquer coisa:
             *
             * 1. valida todas as ContentSourceKeys;
             * 2. valida as sources;
             * 3. executa as requests;
             * 4. coloca o primeiro conteúdo retornado em Response.
             *
             * Isso transforma o ExecutableModel configurado pelo usuário
             * em um modelo realmente executável.
             */
            await ResolveContentSources(executableModel);

            List<string> scenePaths = new();

            /*
             * Áudio principal.
             */
            string? mainAudioPath = null;

            if (executableModel.Project.hasMainAudio())
            {
                var mainAudioKey = new ContentSourceKey(1, 0, 0);

                if (!executableModel.TryGetContentSource(
                        mainAudioKey,
                        out var mainAudioSource))
                {
                    throw new InvalidOperationException(
                        "O modelo possui áudio principal, mas não possui ContentSourceKey (1,0,0)."
                    );
                }

                string audioText = ResolveVariables(
                    executableModel.Project.Prompt,
                    executableModel,
                    mainAudioKey
                );

                if (string.IsNullOrWhiteSpace(audioText))
                {
                    audioText = GetDefaultContentText(
                        mainAudioSource.Response
                    );
                }

                if (string.IsNullOrWhiteSpace(audioText))
                {
                    throw new InvalidOperationException(
                        "Não foi possível obter conteúdo para o áudio principal."
                    );
                }

                mainAudioPath = Path.Combine(
                    basePath,
                    $"dynamic_main_{Guid.NewGuid():N}.wav"
                );

                await audioService.GenerateTemporaryAudio(
                    audioText,
                    mainAudioPath
                );
            }

            /*
             * Renderização das cenas.
             */
            foreach (Scene scene in executableModel.Project.Scenes)
            {
                if (scene == null)
                    throw new InvalidOperationException(
                        "Foi encontrada uma cena nula no modelo."
                    );

                var sceneKey = new ContentSourceKey(
                    null,
                    scene.Id,
                    null
                );

                sceneKey.IsValid();

                if (!executableModel.TryGetContentSource(
                        sceneKey,
                        out var sceneSource))
                {
                    throw new InvalidOperationException(
                        $"A cena {scene.Id} não possui ContentSourceKey registrada."
                    );
                }

                string scenePath = await RenderScene(
                    executableModel,
                    scene,
                    sceneSource,
                    videoService,
                    audioService,
                    imageService,
                    basePath
                );

                scenePaths.Add(scenePath);
            }

            if (scenePaths.Count == 0)
                throw new InvalidOperationException(
                    "Nenhuma cena foi renderizada."
                );

            /*
             * Junta todas as cenas.
             */
            string concatenatedPath = Path.Combine(
                basePath,
                $"dynamic_concat_{Guid.NewGuid():N}.mp4"
            );

            await videoService.ConcatVideosAsync(
                scenePaths.ToArray(),
                concatenatedPath
            );

            /*
             * Adiciona o áudio principal, caso exista.
             */
            string finalPath = Path.Combine(
                dinamicosPath,
                $"{SanitizeFileName(executableModel.Name)}.mp4"
            );

            if (mainAudioPath != null)
            {
                await videoService.AddMainAudioAsync(
                    concatenatedPath,
                    mainAudioPath,
                    finalPath
                );
            }
            else
            {
                File.Copy(
                    concatenatedPath,
                    finalPath,
                    true
                );
            }

            if (!File.Exists(finalPath))
                throw new InvalidOperationException(
                    "A renderização terminou, mas o arquivo MP4 não foi criado."
                );

            return finalPath;
        }

        private static double GetSceneDuration(Scene scene)
        {
            if (scene.Duration == "Inner")
            {
                throw new InvalidOperationException(
                    $"A cena {scene.Id} possui duração 'Inner', " +
                    $"mas sua duração precisa ser conhecida para gerar uma imagem."
                );
            }

            if (scene.Duration == "FromAudio")
            {
                throw new InvalidOperationException(
                    $"A cena {scene.Id} possui duração 'FromAudio', " +
                    $"mas uma imagem precisa de uma duração concreta."
                );
            }

            if (!double.TryParse(
                    scene.Duration,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double duration))
            {
                throw new InvalidOperationException(
                    $"Duração inválida na cena {scene.Id}: '{scene.Duration}'."
                );
            }

            if (duration <= 0)
                throw new InvalidOperationException(
                    $"A duração da cena {scene.Id} deve ser maior que zero."
                );

            /*
             * VideoService.ImageAsync recebe segundos.
             * O modelo trabalha com milissegundos.
             */
            return duration / 1000.0;
        }

        private static double GetLayerDuration(
            Layer layer,
            Scene scene)
        {
            if (layer.Duration == "Inner")
                throw new InvalidOperationException(
                    $"A layer {layer.Id} usa 'Inner' e não possui duração explícita."
                );

            if (layer.Duration == "FromAudio")
                throw new InvalidOperationException(
                    $"A layer {layer.Id} usa 'FromAudio' e precisa de áudio "
                    + "para determinar sua duração."
                );

            if (!double.TryParse(
                    layer.Duration,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double duration))
            {
                throw new InvalidOperationException(
                    $"Duração inválida na layer {layer.Id}: '{layer.Duration}'."
                );
            }

            if (duration <= 0)
                throw new InvalidOperationException(
                    $"A duração da layer {layer.Id} deve ser maior que zero."
                );

            return duration / 1000.0;
        }

        private static string GetDefaultContentText(object? response)
        {
            if (response == null)
                return string.Empty;

            Type type = response.GetType();

            /*
             * Ordem de preferência para as Responses existentes
             * no projeto.
             */
            string[] properties =
            {
        "Result",
        "Description",
        "Title",
        "Link"
    };

            foreach (string propertyName in properties)
            {
                object? value =
                    GetPropertyValue(
                        response,
                        type,
                        propertyName
                    );

                if (value == null)
                    continue;

                string? text = value.ToString();

                if (!string.IsNullOrWhiteSpace(text))
                    return text;
            }

            return string.Empty;
        }

        private static string SanitizeFileName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(
                    "O nome do modelo dinâmico não pode ser vazio."
                );

            foreach (char invalid in Path.GetInvalidFileNameChars())
                name = name.Replace(invalid, '_');

            return name;
        }

        private static double GetStartSeconds(string? start)
        {
            if (string.IsNullOrWhiteSpace(start))
                throw new InvalidOperationException(
                    "O Start da layer não pode ser nulo."
                );

            if (!double.TryParse(
                    start,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double value))
            {
                throw new InvalidOperationException(
                    $"Start inválido: '{start}'."
                );
            }

            if (value < 0)
                throw new InvalidOperationException(
                    "O Start da layer não pode ser negativo."
                );

            /*
             * O modelo usa milissegundos.
             * FFmpeg recebe segundos.
             */
            return value / 1000.0;
        }


        private static async Task<string> RenderScene(
        ExecutableModel executableModel,
        Scene scene,
        (IContentSource? ContentSource,
         object Request,
         object Response) sceneSource,
        VideoService videoService,
        AudioService audioService,
        ImageService imageService,
        string basePath)
        {
            string resolvedPrompt = ResolveVariables(
                scene.Prompt,
                executableModel,
                new ContentSourceKey(null, scene.Id, null)
            );

            string scenePath = Path.Combine(
                basePath,
                $"dynamic_scene_{scene.Id}_{Guid.NewGuid():N}.mp4"
            );

            switch (scene.Type)
            {
                case "Image":
                    {
                        if (string.IsNullOrWhiteSpace(resolvedPrompt))
                        {
                            resolvedPrompt = GetDefaultContentText(
                                sceneSource.Response
                            );
                        }

                        if (string.IsNullOrWhiteSpace(resolvedPrompt))
                            throw new InvalidOperationException(
                                $"A cena {scene.Id} não possui conteúdo para gerar a imagem."
                            );

                        byte[] imageBytes =
                            await imageService.GenerateImage(resolvedPrompt);

                        double duration = GetSceneDuration(scene);

                        await videoService.ImageAsync(
                            imageBytes,
                            duration,
                            scenePath
                        );

                        break;
                    }

                case "Image&Audio":
                    {
                        if (string.IsNullOrWhiteSpace(resolvedPrompt))
                        {
                            resolvedPrompt = GetDefaultContentText(
                                sceneSource.Response
                            );
                        }

                        if (string.IsNullOrWhiteSpace(resolvedPrompt))
                            throw new InvalidOperationException(
                                $"A cena {scene.Id} não possui conteúdo."
                            );

                        byte[] imageBytes =
                            await imageService.GenerateImage(resolvedPrompt);

                        string audioPath = Path.Combine(
                            basePath,
                            $"dynamic_scene_{scene.Id}_{Guid.NewGuid():N}.wav"
                        );

                        await audioService.GenerateTemporaryAudio(
                            resolvedPrompt,
                            audioPath
                        );

                        await videoService.ImageWithAudioAsync(
                            imageBytes,
                            audioPath,
                            scenePath
                        );

                        break;
                    }

                case "Video":
                    {
                        string videoPath =
                            GetDefaultContentText(sceneSource.Response);

                        if (string.IsNullOrWhiteSpace(videoPath))
                            throw new InvalidOperationException(
                                $"A source da cena {scene.Id} não forneceu um caminho de vídeo."
                            );

                        if (!File.Exists(videoPath))
                            throw new FileNotFoundException(
                                $"O vídeo da cena {scene.Id} não existe.",
                                videoPath
                            );

                        byte[] videoBytes =
                            await File.ReadAllBytesAsync(videoPath);

                        if (scene.Duration == "Inner")
                        {
                            await videoService.VideoWithoutDuration(
                                videoBytes,
                                scenePath
                            );
                        }
                        else
                        {
                            double duration = GetSceneDuration(scene);

                            await videoService.VideoWithDuration(
                                videoBytes,
                                duration,
                                scenePath
                            );
                        }

                        break;
                    }

                default:
                    throw new InvalidOperationException(
                        $"Tipo de cena '{scene.Type}' não suportado."
                    );
            }

            /*
             * Layers são processadas sequencialmente.
             * O resultado de uma layer passa a ser a base da próxima.
             */
            string composedPath = scenePath;

            int layerIndex = 0;

            if (scene.Layers != null)
            {
                foreach (Layer layer in scene.Layers)
                {
                    if (layer == null)
                        throw new InvalidOperationException(
                            $"A cena {scene.Id} possui uma layer nula."
                        );

                    var layerKey = new ContentSourceKey(
                        null,
                        scene.Id,
                        layer.Id
                    );

                    layerKey.IsValid();

                    if (!executableModel.TryGetContentSource(
                            layerKey,
                            out var layerSource))
                    {
                        throw new InvalidOperationException(
                            $"A layer {layer.Id} da cena {scene.Id} não possui ContentSourceKey."
                        );
                    }

                    string layerPrompt = ResolveVariables(
                        layer.Prompt,
                        executableModel,
                        layerKey
                    );

                    if (string.IsNullOrWhiteSpace(layerPrompt))
                    {
                        layerPrompt = GetDefaultContentText(
                            layerSource.Response
                        );
                    }

                    string layerPath = await RenderLayer(
                        executableModel,
                        scene,
                        layer,
                        layerSource,
                        layerPrompt,
                        videoService,
                        audioService,
                        imageService,
                        basePath
                    );

                    if (layerPath == null)
                        continue;

                    string nextComposedPath = Path.Combine(
                        basePath,
                        $"dynamic_scene_{scene.Id}_composed_{layerIndex}_{Guid.NewGuid():N}.mp4"
                    );

                    double start = GetStartSeconds(layer.Start);

                    if (scene.Duration == "Inner" ||
                        scene.Duration == "FromAudio")
                    {
                        await videoService.OverlayLayerAsync(
                            composedPath,
                            layerPath,
                            start,
                            nextComposedPath
                        );
                    }
                    else
                    {
                        double duration = GetSceneDuration(scene);

                        await videoService.OverlayLayerWithDurationAsync(
                            composedPath,
                            layerPath,
                            start,
                            duration,
                            nextComposedPath
                        );
                    }

                    composedPath = nextComposedPath;

                    layerIndex++;
                }
            }

            return composedPath;
        }

        private static async Task<string?> RenderLayer(
        ExecutableModel executableModel,
        Scene scene,
        Layer layer,
        (IContentSource? ContentSource,
         object Request,
         object Response) layerSource,
        string prompt,
        VideoService videoService,
        AudioService audioService,
        ImageService imageService,
        string basePath)
        {
            string layerPath = Path.Combine(
                basePath,
                $"dynamic_scene_{scene.Id}_layer_{layer.Id}_{Guid.NewGuid():N}.mp4"
            );

            switch (layer.Type)
            {
                case "Image":
                    {
                        if (string.IsNullOrWhiteSpace(prompt))
                            throw new InvalidOperationException(
                                $"A layer {layer.Id} da cena {scene.Id} não possui conteúdo."
                            );

                        byte[] imageBytes =
                            await imageService.GenerateImage(prompt);

                        double duration = GetLayerDuration(
                            layer,
                            scene
                        );

                        await videoService.ImageAsync(
                            imageBytes,
                            duration,
                            layerPath
                        );

                        return layerPath;
                    }

                case "Image&Audio":
                    {
                        if (string.IsNullOrWhiteSpace(prompt))
                            throw new InvalidOperationException(
                                $"A layer {layer.Id} da cena {scene.Id} não possui conteúdo."
                            );

                        byte[] imageBytes =
                            await imageService.GenerateImage(prompt);

                        string audioPath = Path.Combine(
                            basePath,
                            $"dynamic_scene_{scene.Id}_layer_{layer.Id}_{Guid.NewGuid():N}.wav"
                        );

                        await audioService.GenerateTemporaryAudio(
                            prompt,
                            audioPath
                        );

                        await videoService.ImageWithAudioAsync(
                            imageBytes,
                            audioPath,
                            layerPath
                        );

                        return layerPath;
                    }

                case "Video":
                    {
                        string videoPath =
                            GetDefaultContentText(layerSource.Response);

                        if (string.IsNullOrWhiteSpace(videoPath))
                            throw new InvalidOperationException(
                                $"A layer {layer.Id} não possui caminho de vídeo."
                            );

                        if (!File.Exists(videoPath))
                            throw new FileNotFoundException(
                                $"Vídeo da layer não encontrado.",
                                videoPath
                            );

                        byte[] videoBytes =
                            await File.ReadAllBytesAsync(videoPath);

                        if (layer.Duration == "Inner")
                        {
                            await videoService.VideoWithoutDuration(
                                videoBytes,
                                layerPath
                            );
                        }
                        else
                        {
                            double duration =
                                GetLayerDuration(layer, scene);

                            await videoService.VideoWithDuration(
                                videoBytes,
                                duration,
                                layerPath
                            );
                        }

                        return layerPath;
                    }

                case "Audio":
                    /*
                     * O VideoService atual não possui operação de áudio
                     * como layer. Portanto não devemos fingir que uma
                     * layer Audio foi renderizada.
                     */
                    throw new NotSupportedException(
                        $"Layer Audio ainda não possui operação de composição " +
                        $"na implementação atual do VideoService."
                    );

                default:
                    throw new InvalidOperationException(
                        $"Tipo de layer '{layer.Type}' não suportado."
                    );
            }
        }

        private static string ResolveVariables(
        string? text,
        ExecutableModel executableModel,
        ContentSourceKey currentKey)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            currentKey.IsValid();

            var regex = new Regex(
                @"/\*(?<source>[A-Za-z_][A-Za-z0-9_]*)\.(?<property>[A-Za-z_][A-Za-z0-9_]*)\*/"
            );

            return regex.Replace(
                text,
                match =>
                {
                    string sourceName =
                        match.Groups["source"].Value;

                    string propertyName =
                        match.Groups["property"].Value;

                    /*
                     * "origin" significa a ContentSource associada
                     * ao contexto atual:
                     *
                     * cena -> (null, sceneId, null)
                     * layer -> (null, sceneId, layerId)
                     * origem global -> (0, 0, 0)
                     */
                    ContentSourceKey key;

                    if (sourceName.Equals(
                            "origin",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        key = currentKey;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            $"Origem de variável '{sourceName}' não reconhecida."
                        );
                    }

                    key.IsValid();

                    if (!executableModel.TryGetContentSource(
                            key,
                            out var source))
                    {
                        throw new InvalidOperationException(
                            $"Não existe ContentSource para a chave " +
                            $"({key.GlobalId},{key.SceneId},{key.LayerId})."
                        );
                    }

                    if (source.Response == null)
                    {
                        throw new InvalidOperationException(
                            $"A Response da ContentSource '{source.ContentSource?.Name}' é nula."
                        );
                    }

                    object? value = GetPropertyValue(
                        source.Response,
                        source.Response.GetType(),
                        propertyName
                    );

                    if (value == null)
                    {
                        throw new InvalidOperationException(
                            $"A propriedade '{propertyName}' da Response " +
                            $"'{source.Response.GetType().Name}' é nula ou não existe."
                        );
                    }

                    return value.ToString() ?? string.Empty;
                }
            );
        }

        private static async Task ResolveContentSources(
        ExecutableModel executableModel)
        {
            foreach (var entry in executableModel.ContentSources.ToList())
            {
                ContentSourceKey key = entry.Key;

                key.IsValid();

                var source = entry.Value.ContentSource;
                var request = entry.Value.Request;

                if (source == null)
                {
                    throw new InvalidOperationException(
                        $"A ContentSourceKey ({key.GlobalId},{key.SceneId},{key.LayerId}) possui ContentSource nula."
                    );
                }

                if (request == null)
                {
                    throw new InvalidOperationException(
                        $"A ContentSourceKey ({key.GlobalId},{key.SceneId},{key.LayerId}) possui Request nula."
                    );
                }

                if (!source.RequestType.IsInstanceOfType(request))
                {
                    throw new InvalidOperationException(
                        $"Request inválida para a ContentSource '{source.Name}'. " +
                        $"Esperado: {source.RequestType.Name}; " +
                        $"recebido: {request.GetType().Name}."
                    );
                }

                List<IContent>? contents =
                    await source.GetContent(request);

                if (contents == null || contents.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"A ContentSource '{source.Name}' não retornou conteúdo."
                    );
                }

                IContent? response = contents.FirstOrDefault();

                if (response == null)
                {
                    throw new InvalidOperationException(
                        $"A ContentSource '{source.Name}' retornou um conteúdo nulo."
                    );
                }

                executableModel.ContentSources[key] =
                (
                    source,
                    request,
                    response
                );
            }
        }

        public static async Task<string> SelectOption(List<string> options)
        {
            return await AnsiConsole.Console.PromptAsync(new SelectionPrompt<string>()
                        .Title("Selecione uma das opções:")
                        .AddChoices(options));
        }

        public static async Task<string> SelectOption(string[] options)
        {
            return await AnsiConsole.Console.PromptAsync(new SelectionPrompt<string>()
                        .Title("Selecione uma das opções:")
                        .AddChoices(options));
        }

        public static object? GetPropertyValue(
        object obj,
        Type type,
        string prop)
        {
            if (obj == null)
                return null;

            PropertyInfo? property =
                type.GetProperty(
                    prop,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.IgnoreCase
                );

            return property?.GetValue(obj);
        }

        public static async Task<(IContentSource ContentSource, object? Request, object? Response)> SelectContentSource(List<IContentSource> ContentSources)
        {
            string modelSelected = await SelectOption(ContentSources.Select(x => x.Name).ToList());

            IContentSource ContentSourceSelected = ContentSources.First(x => x.Name == modelSelected);

            return (ContentSourceSelected, await GetRequestForContentSource(ContentSourceSelected.RequestType), await GetResponseForContentSource(ContentSourceSelected.RequestType));
        }

        public static async Task<object?> GetRequestForContentSource(Type RequestType) => Activator.CreateInstance(RequestType);

        public static async Task<object?> GetResponseForContentSource(Type RequestType)
        {

            switch (RequestType.Name)
            {
                case "AiContentSourceRequest":
                    return new AiContentSourceResponse();

                case "GoogleImageContentRequest":
                    return new GoogleImageContentResponse();

                case "RssContentSourceRequest":
                    return new G1RssContentSourceResponse();
            }

            return null;
        }
    }
}