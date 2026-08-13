using System.IO;

using LibGit2Sharp;

using Newtonsoft.Json;

using SimpleWpf.GitManager.Event;
using SimpleWpf.GitManager.Interface;
using SimpleWpf.GitManager.Model;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace SimpleWpf.GitManager.Controller
{
    [IocExport(typeof(IGitController))]
    public class GitController : IGitController
    {
        private readonly IIocEventAggregator _eventAggregator;

        private GitManagerConfiguration _configuration;
        private string _configurationFile;

        bool _isShutdown;
        bool _isDisposed;

        [IocImportingConstructor]
        public GitController(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _configuration = null;
            _isShutdown = false;
            _isDisposed = false;
        }

        public GitManagerConfiguration GetConfiguration()
        {
            return _configuration;
        }
        public string GetConfigurationFile()
        {
            return _configurationFile;
        }
        public string GetConfigurationFullPath()
        {
            return Path.GetFullPath(_configurationFile);
        }

        public async Task Initialize(string configurationFile, string defaultConfigurationFile)
        {
            try
            {
                _configurationFile = configurationFile;
                _configuration = OpenConfiguration();

                // Init Repositories
                if (!string.IsNullOrEmpty(_configuration.Directory))
                {
                    // Git Directories (assume)
                    foreach (var directory in Directory.GetDirectories(_configuration.Directory))
                    {
                        var gitPath = Path.Combine(directory, ".git");
                        var dirInfo = new DirectoryInfo(gitPath);
                        var gitName = Directory.GetParent(gitPath).Name;           // Git naming convention does not name repository itself

                        if (string.IsNullOrWhiteSpace(gitName))
                            continue;

                        if (Directory.Exists(gitPath))
                        {
                            // Load using LibGit2Sharp
                            var gitRepo = new Repository(gitPath, new RepositoryOptions());

                            // Initial Creation
                            if (!_configuration.Repositories.Any(x => x.Name == gitName))
                            {
                                var repository = new GitRepository()
                                {
                                    BaseDirectory = dirInfo.FullName,
                                    IsFork = false,
                                    Name = gitName,
                                    LastCommit = string.Format("Last Commit:  {0}, {1}, {2}",
                                                    gitRepo.Head.Tip.Author.Name,
                                                    gitRepo.Head.Tip.Author.Email,
                                                    gitRepo.Head.Tip.Author.When),
                                    LastAccessLocal = new DateTimeOffset(dirInfo.LastAccessTime),
                                    LastAccessRemote = gitRepo.Head.Tip.Author.When,
                                    GitUrl = gitRepo.Network.Remotes.FirstOrDefault()?.Url ?? "Not Specified"
                                };

                                _configuration.Repositories.Add(repository);
                            }

                            // Already Exists
                            else
                            {
                                var repository = _configuration.Repositories.First(x => x.Name == gitName);

                                repository.LastAccessRemote = gitRepo.Head.Tip.Author.When;
                                repository.LastAccessLocal = new DateTimeOffset(dirInfo.LastAccessTime);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _configurationFile = defaultConfigurationFile;
                _configuration = new GitManagerConfiguration();

                throw new Exception("Initialization failed. Please check configuration", ex);
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

                        _eventAggregator.GetEvent<StatusEvent>().Publish("Configuration Loaded:  " + _configurationFile);

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

                        _eventAggregator.GetEvent<StatusEvent>().Publish("Configuration Saved:  " + _configurationFile);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving configuration", ex);
            }
        }

        public void Shutdown()
        {
            if (_isShutdown)
                throw new Exception("IGitController Shutdown already called!");

            try
            {
                SaveConfiguration();
            }
            catch (Exception ex)
            {
                throw new Exception("Shutdown failed (check that configuration is not locked and you have file permissions)");
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