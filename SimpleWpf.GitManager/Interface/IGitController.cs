using SimpleWpf.Extensions.Event;
using SimpleWpf.GitManager.Model;

namespace SimpleWpf.GitManager.Interface
{
    public interface IGitController : IDisposable
    {
        Task Initialize();

        string GetConfigurationFile();
        string GetConfigurationFullPath();

        GitManagerConfiguration GetConfiguration();
        Task SetConfiguration(SimpleEventHandler<GitManagerConfiguration> callback, bool requiresReload);

        GitRepositoryStub GetRepository(string gitName);
        GitRepositoryLog GetRepositoryLog(string gitName);

        IEnumerable<GitRepositoryStub> GetRepositoryList();

        Task OpenConfiguration(string configurationFile);
        Task SaveConfiguration();

        Task Fetch(GitRepositoryStub repository);
        Task Clone(GitRepositoryStub repository);
    }
}
