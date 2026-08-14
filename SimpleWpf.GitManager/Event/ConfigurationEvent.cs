using SimpleWpf.IocFramework.EventAggregation;

namespace SimpleWpf.GitManager.Event
{
    public enum ConfigurationEventType
    {
        Loaded = 0,
        Saved = 1,
        Modified = 2
    }

    public class ConfigurationEvent : IocEvent<ConfigurationEventType>
    {
    }
}
