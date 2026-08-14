using SimpleWpf.GitManager.Model;
using SimpleWpf.IocFramework.EventAggregation;

namespace SimpleWpf.GitManager.Event
{
    public class ConfigurationLoadedEvent : IocEvent<GitManagerConfiguration>
    {
    }
}
