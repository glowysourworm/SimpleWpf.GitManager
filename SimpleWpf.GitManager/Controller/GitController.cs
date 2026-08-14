using System.IO;

using Newtonsoft.Json;

using SimpleWpf.Extensions.Event;
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
        private readonly IGitRepositoryManager _repositoryManager;
        private readonly IGitLogManager _logManager;

        private GitManagerConfiguration _configuration;
        private string _configurationFile;

        bool _isDisposed;

        [IocImportingConstructor]
        public GitController(IIocEventAggregator eventAggregator,
                             IGitRepositoryManager repositoryManager,
                             IGitLogManager logManager)
        {
            _eventAggregator = eventAggregator;
            _repositoryManager = repositoryManager;
            _logManager = logManager;

            _configuration = null;
            _isDisposed = false;
        }

        public string GetConfigurationFile()
        {
            return _configurationFile;
        }
        public string GetConfigurationFullPath()
        {
            return Path.GetFullPath(_configurationFile);
        }

        public Task SetConfiguration(SimpleEventHandler<GitManagerConfiguration> callback)
        {
            callback(_configuration);

            return SaveConfiguration();
        }

        public void GetConfiguration(SimpleEventHandler<GitManagerConfiguration> callback)
        {
            callback(_configuration);
        }

        public GitRepositoryStub GetRepository(string gitName)
        {
            return _repositoryManager.Get(gitName);
        }
        public IEnumerable<string> GetRepositoryList()
        {
            return _repositoryManager.GetList();
        }

        public Task RemoveAllReposFromConfiguration()
        {
            return Task.Run(async () =>
            {
                // Configuration
                _configuration.Repositories.Clear();

                // Repos
                await _repositoryManager.RemoveAll();

                // Logs
                await _logManager.RemoveAll();

                // -> Save
                await SaveConfiguration();
            });
        }
        public Task ReloadAllReposFromConfiguration()
        {
            return Task.Run(async () =>
            {
                await _repositoryManager.Initialize(_configuration);
            });
        }

        public Task OpenConfiguration(string configurationFile)
        {
            return Task.Run(async () =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(configurationFile))
                        throw new ArgumentNullException("Configuration file not specified (please see command line instructions)");

                    _configurationFile = configurationFile;

                    // Initial Configuration
                    if (!File.Exists(_configurationFile))
                    {
                        File.Create(_configurationFile);
                        await LoadConfiguration(new GitManagerConfiguration());
                        return;
                    }

                    var serializer = new JsonSerializer()
                    {
                        Formatting = Formatting.Indented,
                    };

                    using (var stream = File.OpenRead(_configurationFile))
                    {
                        using (var streamReader = new StreamReader(stream))
                        {
                            using (var reader = new JsonTextReader(streamReader))
                            {
                                var configuration = serializer.Deserialize<GitManagerConfiguration>(reader);

                                // File (or) Default
                                await LoadConfiguration(configuration ?? new GitManagerConfiguration());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
        }

        public Task SaveConfiguration()
        {
            return Task.Run(() =>
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

                    using (var stream = File.OpenWrite(_configurationFile))
                    {
                        using (var streamWriter = new StreamWriter(stream))
                        {
                            using (var writer = new JsonTextWriter(streamWriter))
                            {
                                serializer.Serialize(writer, _configuration);

                                // -> Configuration Event
                                _eventAggregator.GetEvent<ConfigurationEvent>().Publish(ConfigurationEventType.Saved);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
        }

        private Task LoadConfiguration(GitManagerConfiguration configuration)
        {
            return Task.Run(async () =>
            {
                try
                {
                    _configuration = configuration;

                    // -> Initialize IGitRepositoryManager
                    await _repositoryManager.Initialize(_configuration);

                    // -> Initialize IGitLogManager
                    await _logManager.Initialize(_configuration);

                    // -> Configuration Event
                    _eventAggregator.GetEvent<ConfigurationEvent>().Publish(ConfigurationEventType.Loaded);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _repositoryManager.Dispose();
                _logManager.Dispose();

                _isDisposed = true;
            }
        }
    }
}