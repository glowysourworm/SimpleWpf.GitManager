using SimpleGit.Model;

using SimpleWpf.GitManager.Model;

namespace SimpleWpf.GitManager.Interface
{
    public interface IGitRepositoryManager : IDisposable
    {
        /// <summary>
        /// Returns a list of repository names
        /// </summary>
        IEnumerable<GitRepositoryStub> GetList();

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
        Task Initialize(GitManagerConfiguration configuration);

        /// <summary>
        /// Re-initializes from the configuration - refreshing the repository list
        /// </summary>
        Task ReInitialize(GitManagerConfiguration configuration);

        /// <summary>
        /// Calls a remote fetch for the repository
        /// </summary>
        Task Fetch(GitManagerConfiguration configuration, GitRepositoryStub repository, GitHandlers.GitLogHandler logHandler);

        /// <summary>
        /// Calls a clone for the specified repository
        /// </summary>
        Task Clone(GitManagerConfiguration configuration, GitRepositoryStub repository, GitHandlers.GitLogHandler logHandler);
    }
}
