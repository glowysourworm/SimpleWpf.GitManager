using SimpleWpf.IocFramework.EventAggregation;

namespace SimpleWpf.GitManager.Event
{
    public enum ViewEventType
    {
        ConfigurationModified,
        ConfigurationModifiedReload,
        RepositoryViewRequest
    }

    public class ViewEventData
    {
        public ViewEventType Type { get; set; }
        public string RepositoryName { get; set; }

        public ViewEventData()
        {
            this.Type = ViewEventType.ConfigurationModified;
            this.RepositoryName = string.Empty;
        }
    }

    public class ViewEvent : IocEvent<ViewEventData>
    {
    }
}
