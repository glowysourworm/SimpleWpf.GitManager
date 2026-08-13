using SimpleWpf.GitManager.Model;

namespace SimpleWpf.GitManager.Interface
{
    public interface IGitController : IDisposable
    {
        GitManagerConfiguration GetConfiguration();

        bool Initialize(string configurationFile, string defaultConfigurationFile, out Exception exception);

        /// <summary>
        /// Saves configuration data and disposes internal components. Must be called before
        /// disposing of component.
        /// </summary>
        bool Shutdown(out Exception exception);
    }
}
