using System.IO;

using LibGit2Sharp;
using LibGit2Sharp.Handlers;

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
        private readonly IGitLogManager _logManager;

        private GitManagerConfiguration _configuration;
        private string _configurationFile;
        private string _configurationFileDefault;

        bool _isShutdown;
        bool _isDisposed;

        [IocImportingConstructor]
        public GitController(IIocEventAggregator eventAggregator, IGitLogManager logManager)
        {
            _eventAggregator = eventAggregator;
            _logManager = logManager;

            _configuration = null;
            _isShutdown = false;
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

        public void SetConfiguration(SimpleEventHandler<GitManagerConfiguration> callback)
        {
            callback(_configuration);

            SaveConfiguration();
        }

        public Task Initialize(string configurationFile, string defaultConfigurationFile)
        {
            return Task.Run(() =>
            {
                _configurationFile = configurationFile;
                _configurationFileDefault = defaultConfigurationFile;

                InitializeImpl();
            });
        }
        public Task RemoveAllReposFromConfiguration()
        {
            return Task.Run(() =>
            {
                // Logs
                foreach (var repository in _configuration.Repositories)
                {
                    _logManager.RemoveLog(repository.Name);
                }

                // Repos
                _configuration.Repositories.Clear();

                // -> Save
                SaveConfiguration();
            });
        }
        public Task ReloadAllReposFromConfiguration()
        {
            return Task.Run(() =>
            {
                InitializeImpl();
            });
        }

        public Task<GitRepository?> Fetch(string gitPath, string gitUrl)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (var gitRepo = new Repository(gitPath))
                    {
                        var credentialsCallback = new CredentialsHandler((user, pass, types) =>
                        {
                            return new UsernamePasswordCredentials()
                            {
                                Username = user,
                                Password = pass
                            };
                        });
                        var progressCallback = new TransferProgressHandler(progress =>
                        {
                            return true;
                        });

                        var logMessage = string.Empty;



                        Commands.Fetch(gitRepo, gitRepo.Head.RemoteName, gitRepo.Refs.Select(x => x.TargetIdentifier), new FetchOptions()
                        {
                            CredentialsProvider = credentialsCallback,
                            Prune = false,
                            OnTransferProgress = progressCallback

                        }, logMessage);

                        return UpdateOrAdd(gitRepo, true, true);
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            });
        }

        // Updates configuration and returns Libgit2Sharp repository object
        private GitRepository UpdateOrAdd(Repository gitRepo, bool isFetch, bool publishEvent)
        {
            if (!Directory.Exists(gitRepo.Info.Path))
                throw new Exception("Git repository doesn't exist locally. Please do a clone first");

            GitRepository repository = null;

            var baseInfo = new DirectoryInfo(gitRepo.Info.Path);
            var gitName = baseInfo.Parent.Name;

            // Initial Creation
            if (!_configuration.Repositories.Any(x => x.Name == gitName))
            {
                repository = new GitRepository()
                {
                    BaseDirectory = gitRepo.Info.WorkingDirectory,
                    IsFork = false,
                    IsHeadUpToDate = false,
                    Name = gitName,
                    LastCommitLocal = string.Format("{0}, {1}, {2}",
                                    gitRepo.Head.Tip.Author.Name,
                                    gitRepo.Head.Tip.Author.Email,
                                    gitRepo.Head.Tip.Author.When),
                    LastCommitRemote = string.Format("{0}, {1}, {2}",
                                    gitRepo.Head.Tip.Author.Name,
                                    gitRepo.Head.Tip.Author.Email,
                                    gitRepo.Head.Tip.Author.When),
                    GitUrl = gitRepo.Network.Remotes.FirstOrDefault()?.Url ?? "Not Specified"
                };

                _configuration.Repositories.Add(repository);
            }

            // Already Exists
            else
            {
                repository = _configuration.Repositories.First(x => x.Name == gitName);

                repository.LastCommitLocal = string.Format("{0}, {1}, {2}",
                                gitRepo.Head.Tip.Author.Name,
                                gitRepo.Head.Tip.Author.Email,
                                gitRepo.Head.Tip.Author.When);
                repository.LastCommitRemote = string.Format("{0}, {1}, {2}",
                                gitRepo.Head.Tip.Author.Name,
                                gitRepo.Head.Tip.Author.Email,
                                gitRepo.Head.Tip.Author.When);
            }

            // Fetch time not stored in repository (or I haven't found it yet)
            if (isFetch)
                repository.LastFetch = DateTime.Now;

            // -> Configuration Loaded Event
            if (publishEvent)
                _eventAggregator.GetEvent<ConfigurationLoadedEvent>().Publish(_configuration);

            return _configuration.Repositories.First(x => x.Name == gitName);
        }

        private void OpenConfiguration(bool publishEvent)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_configurationFile))
                    throw new ArgumentNullException("Configuration file not specified (please see command line instructions)");

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

                            if (configuration == null)
                                throw new Exception("Configuration file read error!");

                            _configuration = configuration;

                            // -> Configuration Loaded Event
                            if (publishEvent)
                                _eventAggregator.GetEvent<ConfigurationLoadedEvent>().Publish(_configuration);
                        }
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

                using (var stream = File.OpenWrite(_configurationFile))
                {
                    using (var streamWriter = new StreamWriter(stream))
                    {
                        using (var writer = new JsonTextWriter(streamWriter))
                        {
                            serializer.Serialize(writer, _configuration);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error saving configuration", ex);
            }
        }

        private void InitializeImpl()
        {
            try
            {
                OpenConfiguration(false);

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
                            using (var gitRepo = new Repository(gitPath, new RepositoryOptions()))
                            {
                                UpdateOrAdd(gitRepo, false, false);
                            }
                        }
                    }
                }

                // -> Configuration Loaded Event
                _eventAggregator.GetEvent<ConfigurationLoadedEvent>().Publish(_configuration);
            }
            catch (Exception ex)
            {
                _configuration = new GitManagerConfiguration();

                throw new Exception("Initialization failed. Please check configuration", ex);
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