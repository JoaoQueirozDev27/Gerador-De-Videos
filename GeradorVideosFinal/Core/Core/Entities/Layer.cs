using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Layer
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } // Audio, Image

        [JsonPropertyName("mix")]
        public int? Mix { get; set; } // 1: toca junto com o main, null: pausa o main

        [JsonPropertyName("duration")]
        public string Duration { get; set; }

        [JsonPropertyName("start")]
        public string Start { get; set; } // Relativo ao início da cena

        [JsonPropertyName("Prompt")]
        public string Prompt { get; set; }

        [JsonPropertyName("Subtitles")]
        public bool Subtitles { get; set; }
    }
}
