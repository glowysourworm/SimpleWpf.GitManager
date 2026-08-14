using SimpleWpf.GitManager.Model;
using SimpleWpf.IocFramework.EventAggregation;

namespace SimpleWpf.GitManager.Event
{
    public class LogEventData
    {
        public string RepositoryName { get; set; }
        public GitRepositoryLogData Data { get; set; }
    }
    public class LogEvent : IocEvent<LogEventData>
    {
    }
}
