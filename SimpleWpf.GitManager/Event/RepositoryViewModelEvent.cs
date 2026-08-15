using SimpleWpf.IocFramework.EventAggregation;

namespace SimpleWpf.GitManager.Event
{
    /// <summary>
    /// Event that occurs when the front end repository must reload. This should
    /// include log messages
    /// </summary>
    public class RepositoryViewModelEvent : IocEvent<RepositoryEventData>
    {
    }
}
