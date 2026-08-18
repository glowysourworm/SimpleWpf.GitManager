using SimpleWpf.GitManager.Model;

namespace SimpleWpf.GitManager.Interface
{
    public interface IGitLogManager : IDisposable
    {
        /// <summary>
        /// Initialize the repository manager with a new configuration
        /// </summary>
        void Initialize(IEnumerable<GitRepositoryStub> repositories);

        /// <summary>
        /// Loads git repository from local path
        /// </summary>
        GitRepositoryLog Get(string repositoryName);

        /// <summary>
        /// Returns true if the repository log exists
        /// </summary>
        bool Exists(string repositoryName);

        /// <summary>
        /// Adds log for specified repository
        /// </summary>
        void Add(string repositoryName);

        /// <summary>
        /// Resets log for a repository
        /// </summary>
        void Clear(string repositoryName);

        /// <summary>
        /// Removes log and log file for repository
        /// </summary>
        void Remove(string repositoryName);

        /// <summary>
        /// Removes all logs and log files for all repositories
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// Process for getting rid of old log files - should be run with the IGitController managing
        /// which repositories are still wanted in the configuration
        /// </summary>
        void RemoveUnused(IEnumerable<GitRepositoryStub> repositories);

        /// <summary>
        /// Logs message for a repository
        /// </summary>
        void Log(string repositoryName, string logMessage);

        /// <summary>
        /// Logs messages for a repository
        /// </summary>
        void Log(string repositoryName, IEnumerable<string> logMessages);
    }
}
