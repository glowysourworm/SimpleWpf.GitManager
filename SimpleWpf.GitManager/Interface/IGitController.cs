using SimpleWpf.Extensions.Event;
using SimpleWpf.GitManager.Model;

namespace SimpleWpf.GitManager.Interface
{
    public interface IGitController : IDisposable
    {
        string GetConfigurationFile();
        string GetConfigurationFullPath();

        void SetConfiguration(SimpleEventHandler<GitManagerConfiguration> callback);

        Task Initialize(string configurationFile, string defaultConfigurationFile);
        Task RemoveAllReposFromConfiguration();
        Task ReloadAllReposFromConfiguration();

        /// <summary>
        /// Performs a basic fetch of the repository; and logs the results
        /// </summary>
        Task<GitRepository?> Fetch(string gitPath, string gitUrl);

        /// <summary>
        /// Saves configuration data and disposes internal components. Must be called before
        /// disposing of component.
        /// </summary>
        void Shutdown();
    }
}
