using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Scene
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } // image&Audio, image, transition, video

        [JsonPropertyName("duration")]
        public string Duration { get; set; } // "5000", "FromAudio", "Inner"

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("subtitles")]
        public bool Subtitles { get; set; }

        [JsonPropertyName("layers")]
        public List<Layer> Layers { get; set; } = new List<Layer>();

        [JsonPropertyName("Prompt")]
        public string Prompt { get; set; }
    }
}
