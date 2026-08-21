using Domain.Interfaces;
using Domain.ValueObjects;

namespace Domain.Entities
{
    public abstract class ExecutableModel
    {
        public List<Content> Origin { get; set; } = new List<Content>();
        public VideoProject Project { get;}
        public Dictionary<ContentSourceKey, IContentSource?> ContentSources
            = new Dictionary<ContentSourceKey, IContentSource?>();

        public ExecutableModel(VideoProject VideoProject)
        {
            Project = VideoProject;
        }
    }
}