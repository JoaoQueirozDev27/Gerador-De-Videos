using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    internal class ExecutableModel
    {
        public enum ContentKind { Image, AudioPath, VideoPath, Text }

        public class ContentItem
        {
            public ContentKind Kind { get; init; }
            public byte[]? ImageBytes { get; init; }
            public string? PathValue { get; init; }   // áudio ou vídeo
            public string? TextValue { get; init; }   // texto pra TTS/legenda

            public static ContentItem FromImage(byte[] bytes) =>
                new() { Kind = ContentKind.Image, ImageBytes = bytes };

            public static ContentItem FromAudioPath(string path) =>
                new() { Kind = ContentKind.AudioPath, PathValue = path };

            public static ContentItem FromVideoPath(string path) =>
                new() { Kind = ContentKind.VideoPath, PathValue = path };

            public static ContentItem FromText(string text) =>
                new() { Kind = ContentKind.Text, TextValue = text };
        }

        public class DynamicModel
        {
            public string VariationName { get; init; } = "";

            // chave = ContentKey da Scene/Layer no ME
            public Dictionary<string, ContentItem> Content { get; init; } = new();

            // variáveis de texto pro /*varName*/ dentro de Prompt (áudio principal, cenas)
            public Dictionary<string, string> PromptVariables { get; init; } = new();

            public ContentItem? Resolve(string? contentKey)
            {
                if (contentKey is null) return null;
                return Content.TryGetValue(contentKey, out var item) ? item : null;
            }
        }
    }
}
