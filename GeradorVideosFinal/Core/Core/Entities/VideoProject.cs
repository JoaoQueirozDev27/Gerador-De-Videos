using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class VideoProject
    {
        [JsonPropertyName("template_name")]
        public string TemplateName { get; set; }

        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("resolution")]
        public string Resolution { get; set; } // 1: 9/16, 2: 16/9

        [JsonPropertyName("audio")]
        public bool AudioConfig { get; set; } // 1: Main audio on, 0: Off

        [JsonPropertyName("scenes")]
        public List<Scene> Scenes { get; set; } = new List<Scene>();

        [JsonPropertyName("Prompt")]
        public string Prompt { get; set; }
    }
}
