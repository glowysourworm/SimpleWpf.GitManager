using SimpleWpf.GitManager.Model;

namespace SimpleWpf.GitManager.Interface
{
    public interface IGitController : IDisposable
    {
        GitManagerConfiguration GetConfiguration();
        string GetConfigurationFile();
        string GetConfigurationFullPath();

        Task Initialize(string configurationFile, string defaultConfigurationFile);

        /// <summary>
        /// Saves configuration data and disposes internal components. Must be called before
        /// disposing of component.
        /// </summary>
        void Shutdown();
    }
}
