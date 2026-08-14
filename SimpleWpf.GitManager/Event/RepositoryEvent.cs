using SimpleWpf.IocFramework.EventAggregation;

namespace SimpleWpf.GitManager.Event
{
    public enum RepositoryEventType
    {
        /// <summary>
        /// Repository was added to the IGitRepositoryManager
        /// </summary>
        Add,

        /// <summary>
        /// Repository data was loaded from disk
        /// </summary>
        Load,

        /// <summary>
        /// Repository was removed from the IGitRepositoryManager
        /// </summary>
        Remove,

        /// <summary>
        /// Repository list was cleared (during a refresh)
        /// </summary>
        RemoveAll,

        /// <summary>
        /// Repository was fetched from remote (origin or master)
        /// </summary>
        Fetch
    }

    public class RepositoryEvent : IocEvent<RepositoryEventType>
    {
    }
}
