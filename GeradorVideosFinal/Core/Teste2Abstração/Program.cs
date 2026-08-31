using Domain.Entities;
using Domain.Interfaces;
using Domain.ValueObjects;
using Services.ContentSources.AiContentSource;
using Services.ContentSources.GoogleImageContent;
using Services.ContentSources.RssContentSource;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Reflection;

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

            Spectre.Console.AnsiConsole.Console.Write(new FigletText("Video").Color(Color.Yellow));
            Spectre.Console.AnsiConsole.Console.Write(new FigletText("Generator").Color(Color.Yellow));
            string BasePath = "C:\\Users\\Administrador\\Desktop\\Modelos";

            AnsiConsole.MarkupLine("[yellow]Este é seu [/] [bold yellow]Gerador de vídeos[/]");

            bool sair = false;

            while (!sair)
            {
                string option = await AnsiConsole.Console.PromptAsync(new SelectionPrompt<string>()
                .Title("Selecione uma das opções:")
                .AddChoices(new[] { "Criar um modelo estático(ME)", "Criar um modelo dinâmico(MD)", "Sair" }));

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

                        




                        break;
                }
            }
        }

        public static async Task<string> SelectOption(List<string> options)
        {
            return await AnsiConsole.Console.PromptAsync(new SelectionPrompt<string>()
                        .Title("Selecione uma das opções:")
                        .AddChoices(options));
        }

        public static object? GetPropertyValue(object obj,Type type,string prop)
        {
            PropertyInfo? propriedade = type.GetProperty(prop);

            if (propriedade != null)
                return propriedade.GetValue(obj);

            return null;
        }

        public static async Task<(IContentSource ContentSource, object? Request, object? Response)> SelectContentSource(List<IContentSource> ContentSources)
        {
            string modelSelected = await SelectOption(ContentSources.Select(x => x.Name).ToList());

            IContentSource ContentSourceSelected = ContentSources.First(x => x.Name == modelSelected);

            return (ContentSourceSelected, await GetRequestForContentSource(ContentSourceSelected.RequestType), await GetResponseForContentSource(ContentSourceSelected.RequestType));
        }

        public static async Task<object?> GetRequestForContentSource(Type RequestType)
        {
            object? request = null;

            switch (RequestType.Name)
            {
                case "AiContentSourceRequest":
                    string prompt = await AnsiConsole.Console.AskAsync<string>("Digite o prompt para a IA(use /*nomeVariavel*/ para variáveis): ");
                    request = new AiContentSourceRequest(prompt, null);
                    break;

                case "GoogleImageContentRequest":
                    string query = await AnsiConsole.Console.AskAsync<string>("Digite a query para o Google Images: ");
                    request = new GoogleImageContentRequest(query);
                    break;

                case "RssContentSourceRequest":
                    string url = await AnsiConsole.Console.AskAsync<string>("Digite a URL do RSS: ");
                    request = new RssContentSourceRequest(url);
                    break;
            }

            return request;
        }

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