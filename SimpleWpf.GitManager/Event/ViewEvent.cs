using SimpleWpf.IocFramework.EventAggregation;

namespace SimpleWpf.GitManager.Event
{
    public enum ViewEventType
    {
        ConfigurationModified,
        ConfigurationModifiedReload
    }

    public class ViewEventData
    {
        public ViewEventType Type { get; set; }

        public ViewEventData()
        {
            this.Type = ViewEventType.ConfigurationModified;
        }
    }

    public class ViewEvent : IocEvent<ViewEventData>
    {
    }
}
