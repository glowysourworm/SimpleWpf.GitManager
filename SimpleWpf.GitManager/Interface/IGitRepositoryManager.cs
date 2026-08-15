using LibGit2Sharp.Handlers;

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
        /// Gets all repositories from the manager
        /// </summary>
        IEnumerable<GitRepositoryStub> GetAll();

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
        void Fetch(string gitName, string user, string password, ProgressHandler progressHandler);

        /// <summary>
        /// Removes all git repositories from the manager
        /// </summary>
        void RemoveAll();
    }
}
