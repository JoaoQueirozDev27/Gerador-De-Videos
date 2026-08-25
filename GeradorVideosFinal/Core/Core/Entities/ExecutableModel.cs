using Domain.Interfaces;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public class ExecutableModel
    {
        public List<Content> Origin { get; set; } = new List<Content>();
        public VideoProject Project { get;}
        public Dictionary<ContentSourceKey, IContentSource?> ContentSources
            = new Dictionary<ContentSourceKey, IContentSource?>();

        public ExecutableModel(VideoProject VideoProject)
        {
            Project = VideoProject;
        }

        public void AddContentSource(
           ContentSourceKey key,
           IContentSource contentSource)
        {
            if (!key.IsValid())
                throw new ArgumentException(
                    "ContentSourceKey inválido.",
                    nameof(key));

            ArgumentNullException.ThrowIfNull(contentSource);

            ContentSources[key] = contentSource;
        }

        public bool TryGetContentSource(
            ContentSourceKey key,
            out IContentSource? contentSource)
        {
            return ContentSources.TryGetValue(
                key,
                out contentSource);
        }
    }
}