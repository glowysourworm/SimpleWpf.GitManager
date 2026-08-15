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

    public class RepositoryEventData
    {
        public string RepositoryName { get; set; }
        public RepositoryEventType EventType { get; set; }

        public RepositoryEventData()
        {
            this.RepositoryName = string.Empty;
            this.EventType = RepositoryEventType.Load;
        }
    }

    public class RepositoryEvent : IocEvent<RepositoryEventData>
    {
    }
}
