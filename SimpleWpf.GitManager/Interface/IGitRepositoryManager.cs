using SimpleWpf.GitManager.Model;

namespace SimpleWpf.GitManager.Interface
{
    public interface IGitRepositoryManager : IDisposable
    {
        /// <summary>
        /// Returns a list of repository names
        /// </summary>
        IEnumerable<string> GetList();

        /// <summary>
        /// Gets repository information from the manager
        /// </summary>
        GitRepositoryStub Get(string gitName);

        /// <summary>
        /// Initialize the repository manager with a new configuration
        /// </summary>
        Task Initialize(GitManagerConfiguration configuration);

        /// <summary>
        /// Loads git repository from local path
        /// </summary>
        Task Load(string gitPath);

        /// <summary>
        /// Loads git repository from local path, then calls remote fetch
        /// </summary>
        Task Fetch(string gitPath);

        /// <summary>
        /// Removes all git repositories from the manager
        /// </summary>
        Task RemoveAll();
    }
}
