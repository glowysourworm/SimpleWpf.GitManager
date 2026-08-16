using SimpleGit;

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
        GitRepository Get(string gitName);

        /// <summary>
        /// Gets all repositories from the manager
        /// </summary>
        IEnumerable<GitRepository> GetAll();

        /// <summary>
        /// Initialize the repository manager with a new configuration
        /// </summary>
        void Initialize(GitManagerConfiguration configuration);

        /// <summary>
        /// Loads git repository from local path
        /// </summary>
        void Load(string gitPath);

        /// <summary>
        /// Loads git repository from local path, then calls remote fetch
        /// </summary>
        void Fetch(string gitName, string userName, string password, GitHandlers.GitLogHandler logHandler);

        /// <summary>
        /// Removes all git repositories from the manager
        /// </summary>
        void RemoveAll();
    }
}
