using System.IO;

using Newtonsoft.Json;

using SimpleWpf.GitManager.Interface;
using SimpleWpf.GitManager.Model;
using SimpleWpf.IocFramework.Application.Attribute;

namespace SimpleWpf.GitManager.Controller
{
    [IocExport(typeof(IGitController))]
    public class GitController : IGitController
    {
        private GitManagerConfiguration _configuration;
        private string _configurationFile;

        bool _isShutdown;
        bool _isDisposed;

        [IocImportingConstructor]
        public GitController()
        {
            _configuration = null;
            _isShutdown = false;
            _isDisposed = false;
        }

        public GitManagerConfiguration GetConfiguration()
        {
            return _configuration;
        }

        public bool Initialize(string configurationFile, string defaultConfigurationFile, out Exception exception)
        {
            try
            {
                exception = null;

                _configurationFile = configurationFile;
                _configuration = OpenConfiguration();

                return true;
            }
            catch (Exception ex)
            {
                _configurationFile = defaultConfigurationFile;
                _configuration = new GitManagerConfiguration();

                exception = ex;
                return false;
            }
        }

        private GitManagerConfiguration OpenConfiguration()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_configurationFile))
                    throw new ArgumentNullException("Configuration file not specified (please see command line instructions)");

                var serializer = new JsonSerializer()
                {
                    Formatting = Formatting.Indented,
                };

                using (var streamReader = new StreamReader(File.OpenRead(_configurationFile)))
                {
                    using (var reader = new JsonTextReader(streamReader))
                    {
                        var configuration = serializer.Deserialize<GitManagerConfiguration>(reader);

                        if (configuration == null)
                            throw new Exception("Configuration file read error!");

                        return configuration;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading configuration", ex);
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_configurationFile))
                    throw new ArgumentNullException("Configuration file not specified (please see command line instructions)");


                if (File.Exists(_configurationFile))
                    File.Delete(_configurationFile);

                var serializer = new JsonSerializer()
                {
                    Formatting = Formatting.Indented,
                };

                using (var streamWriter = new StreamWriter(File.OpenWrite(_configurationFile)))
                {
                    using (var writer = new JsonTextWriter(streamWriter))
                    {
                        serializer.Serialize(writer, _configuration);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving configuration", ex);
            }
        }

        public bool Shutdown(out Exception exception)
        {
            if (_isShutdown)
            {
                exception = new Exception("IGitController Shutdown already called!");
                return false;
            }

            try
            {
                exception = null;

                SaveConfiguration();

                return true;
            }
            catch (Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public void Dispose()
        {
            if (!_isShutdown)
            {
                throw new Exception("Must first call IGitController.Shutdown before disposing");
            }

            if (!_isDisposed)
            {
                _isDisposed = true;
            }
        }
    }
}