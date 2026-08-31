using Domain.Interfaces;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class ExecutableModel
    {
        public VideoProject Project { get;}
        public string Name { get; set; } = string.Empty;

        public Dictionary<ContentSourceKey, (IContentSource? ContentSource,object Request,object Response)> ContentSources { get; set; }
            = new Dictionary<ContentSourceKey, (IContentSource? ContentSource, object Request, object Response)>();

        public ExecutableModel(VideoProject VideoProject)
        {
            Project = VideoProject;
        }

        public void AddContentSource(
           ContentSourceKey key,
           (IContentSource? ContentSource, object Request, object Response) contentSource)
        {
            if (!key.IsValid())
                throw new ArgumentException(
                    "ContentSourceKey inválido.",
                    nameof(key));

            ContentSources[key] = contentSource;
        }

        public bool TryGetContentSource(
            ContentSourceKey key,
            out (IContentSource? ContentSource, object Request, object Response) contentSource)
        {
            return ContentSources.TryGetValue(
                key,
                out contentSource);
        }
    }
}