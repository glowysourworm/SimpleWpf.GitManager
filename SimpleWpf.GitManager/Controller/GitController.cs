using System.IO;
using System.Windows.Threading;

using Newtonsoft.Json;

using SimpleGit.Model;

using SimpleWpf.Extensions.Event;
using SimpleWpf.GitManager.Event;
using SimpleWpf.GitManager.Interface;
using SimpleWpf.GitManager.Model;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.Utilities;

namespace SimpleWpf.GitManager.Controller
{
    [IocExport(typeof(IGitController))]
    public class GitController : IGitController
    {
        private readonly IIocEventAggregator _eventAggregator;
        private readonly IGitRepositoryManager _repositoryManager;
        private readonly IGitLogManager _logManager;

        // Configuration (Primary)
        //
        private GitManagerConfiguration _configuration;
        string _configurationFile;

        bool _isDisposed;

        [IocImportingConstructor]
        public GitController(IIocEventAggregator eventAggregator,
                             IGitRepositoryManager repositoryManager,
                             IGitLogManager logManager)
        {
            _eventAggregator = eventAggregator;
            _repositoryManager = repositoryManager;
            _logManager = logManager;

            _isDisposed = false;

            eventAggregator.GetEvent<RepositoryEvent>().Subscribe(data =>
            {
                BasicHelpers.InvokeDispatcher(OnRepositoryEvent, DispatcherPriority.Background, data);
            });
        }

        public string GetConfigurationFile()
        {
            return _configurationFile;
        }
        public string GetConfigurationFullPath()
        {
            return Path.GetFullPath(_configurationFile);
        }

        public Task SetConfiguration(SimpleEventHandler<GitManagerConfiguration> callback, bool requiresReload)
        {
            callback(_configuration);

            return Task.Run(async () =>
            {
                await SaveConfiguration();

                if (requiresReload)
                {
                    // Repos -> Log (event aggregator)
                    await _repositoryManager.ReInitialize(_configuration);
                }
            });
        }

        public void GetConfiguration(SimpleEventHandler<GitManagerConfiguration> callback)
        {
            callback(_configuration);
        }

        public GitRepositoryStub GetRepository(string gitName)
        {
            return _repositoryManager.Get(gitName);
        }
        public GitRepositoryLog GetRepositoryLog(string gitName)
        {
            return _logManager.Get(gitName);
        }
        public IEnumerable<GitRepositoryStub> GetRepositoryList()
        {
            return _repositoryManager.GetList();
        }

        public Task Fetch(GitRepositoryStub repository)
        {
            return Task.Run(async () =>
            {
                await _repositoryManager.Fetch(new GitRepositoryRequest()
                {
                    BaseDirectory = _configuration.Directory,
                    Password = _configuration.Password,
                    RepositoryId = repository.Id,
                    RepositoryName = repository.Name,
                    Type = GitRepositoryRequest.RequestType.Fetch,
                    Url = repository.Url,
                    User = _configuration.User,
                    WorkingDirectory = repository.WorkingDirectory

                }, logMessage =>
                {
                    // Libgit2Sharp:  Log messages sometimes have several lines at once
                    //                sent back from the Git proxy

                    if (!string.IsNullOrWhiteSpace(logMessage))
                    {
                        var logLines = logMessage.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                        foreach (var message in logLines)
                        {
                            _logManager.Log(repository.Name, logMessage);
                        }
                    }

                    // Git cancel option (true for continue)
                    return true;
                });
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
                        LoadConfiguration(new GitManagerConfiguration());
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
                                LoadConfiguration(configuration ?? new GitManagerConfiguration());
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
                                // _eventAggregator.GetEvent<ConfigurationEvent>().Publish(ConfigurationEventType.Saved);
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

        private void LoadConfiguration(GitManagerConfiguration configuration)
        {
            try
            {
                _configuration = configuration;

                // -> Initialize IGitRepositoryManager -> Logs (event aggregator)
                _repositoryManager.Initialize(_configuration);

                // -> Configuration Event
                _eventAggregator.GetEvent<ConfigurationEvent>().Publish(ConfigurationEventType.Loaded);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void OnRepositoryEvent(RepositoryEventData data)
        {
            // Procedure:  Use this to serialize loading of repositories
            //
            // 0) IGitRepositoryManager loaded repository
            // 1) IGitLogManager loads repository log
            // 2) Send event to the front end

            switch (data.EventType)
            {
                case RepositoryEventType.Add:
                {
                    if (!_logManager.Exists(data.RepositoryName))
                        _logManager.Add(data.RepositoryName);
                }
                break;
                case RepositoryEventType.Remove:
                {
                    if (_logManager.Exists(data.RepositoryName))
                        _logManager.Remove(data.RepositoryName);
                }
                break;
                case RepositoryEventType.RemoveAll:
                {
                    _logManager.RemoveAll();
                }
                break;
                case RepositoryEventType.Update:
                case RepositoryEventType.Fetch:
                    break;
                default:
                    throw new Exception("Unhandled Repository Event Type");
            }

            _eventAggregator.GetEvent<RepositoryViewModelEvent>().Publish(data);
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