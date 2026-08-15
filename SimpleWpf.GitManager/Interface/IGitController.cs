using SimpleWpf.Extensions.Event;
using SimpleWpf.GitManager.Model;

namespace SimpleWpf.GitManager.Interface
{
    public interface IGitController : IDisposable
    {
        string GetConfigurationFile();
        string GetConfigurationFullPath();

        void GetConfiguration(SimpleEventHandler<GitManagerConfiguration> callback);
        Task SetConfiguration(SimpleEventHandler<GitManagerConfiguration> callback, bool requiresReload);

        GitRepositoryStub GetRepository(string gitName);
        GitRepositoryLog GetRepositoryLog(string gitName);

        IEnumerable<string> GetRepositoryList();

        Task OpenConfiguration(string configurationFile);
        Task SaveConfiguration();

        Task Fetch(string repositoryName);
    }
}
