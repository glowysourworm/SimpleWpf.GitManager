using SimpleWpf.GitManager.Model;

namespace SimpleWpf.GitManager.Interface
{
    public interface IGitLogManager : IDisposable
    {
        /// <summary>
        /// Initialize the repository manager with a new configuration
        /// </summary>
        Task Initialize(GitManagerConfiguration configuration);

        /// <summary>
        /// Loads git repository from local path
        /// </summary>
        GitRepositoryLog Get(string repositoryName);

        /// <summary>
        /// Resets log for a repository
        /// </summary>
        void Clear(string repositoryName);

        /// <summary>
        /// Removes log and log file for repository
        /// </summary>
        Task Remove(string repositoryName);

        /// <summary>
        /// Removes all logs and log files for all repositories
        /// </summary>
        Task RemoveAll();

        /// <summary>
        /// Process for getting rid of old log files - should be run with the IGitController managing
        /// which repositories are still wanted in the configuration
        /// </summary>
        Task RemoveUnused(GitManagerConfiguration configuration);

        /// <summary>
        /// Logs message for a repository
        /// </summary>
        Task Log(string repositoryName, string logMessage);

        /// <summary>
        /// Logs messages for a repository
        /// </summary>
        Task Log(string repositoryName, IEnumerable<string> logMessages);
    }
}
