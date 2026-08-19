using Domain.Interfaces;

namespace Domain.Entities
{
    public abstract class ExecutableModel
    {
        public List<Content> origin { get; set; }
        public VideoProject project { get;}
        public Dictionary<(int? globalId,int? sceneId, int? layerId), IContentSource?> contentSources
            = new Dictionary<(int? globalId,int? sceneId, int? layerId), IContentSource?>();

        public ExecutableModel(VideoProject videoProject)
        {
            project = videoProject;
        }
    }
}