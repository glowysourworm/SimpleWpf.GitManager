using System.IO;

using LibGit2Sharp;
using LibGit2Sharp.Handlers;

using SimpleWpf.GitManager.Event;
using SimpleWpf.GitManager.Interface;
using SimpleWpf.GitManager.Model;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace SimpleWpf.GitManager.Component
{
    [IocExport(typeof(IGitRepositoryManager))]
    public class GitRepositoryManager : IGitRepositoryManager
    {
        private readonly IIocEventAggregator _eventAggregator;
        private readonly IGitLogManager _logManager;

        private List<GitRepositoryStub> _repositories;

        [IocImportingConstructor]
        public GitRepositoryManager(IIocEventAggregator eventAggregator, IGitLogManager logManager)
        {
            _eventAggregator = eventAggregator;
            _logManager = logManager;
            _repositories = new List<GitRepositoryStub>();
        }

        public GitRepositoryStub Get(string gitName)
        {
            return _repositories.First(x => x.Name == gitName);
        }

        public IEnumerable<string> GetList()
        {
            return _repositories.Select(r => r.Name).ToList();
        }

        public Task Fetch(string gitPath)
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

                        var progressLogCallback = new ProgressHandler(serverOutput =>
                        {
                            // TODO: Show Output Log

                            return true;
                        });

                        Commands.Fetch(gitRepo, gitRepo.Head.RemoteName, gitRepo.Refs.Select(x => x.TargetIdentifier), new FetchOptions()
                        {
                            CredentialsProvider = credentialsCallback,
                            Prune = false,
                            OnTransferProgress = progressCallback,
                            OnProgress = progressLogCallback

                        }, "Fetching from GitManager:  " + DateTime.Now.ToString());

                        // Repository Event (Fetch)
                        _eventAggregator.GetEvent<RepositoryEvent>().Publish(RepositoryEventType.Fetch);

                        return UpdateOrAdd(gitRepo, true);
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            });
        }

        public Task Initialize(GitManagerConfiguration configuration)
        {
            return Task.Run(async () =>
            {
                try
                {
                    // Loaded Repositories (remove and repopulate)
                    _repositories.Clear();

                    // Init Repositories
                    if (!string.IsNullOrEmpty(configuration.Directory))
                    {
                        // Git Directories (assume)
                        foreach (var directory in Directory.GetDirectories(configuration.Directory))
                        {
                            var gitPath = Path.Combine(directory, ".git");
                            var dirInfo = new DirectoryInfo(gitPath);
                            var gitName = Directory.GetParent(gitPath).Name;           // Git naming convention does not name repository itself

                            if (string.IsNullOrWhiteSpace(gitName))
                                continue;

                            await Load(gitPath);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });
        }

        public Task Load(string gitPath)
        {
            return Task.Run(async () =>
            {
                if (Directory.Exists(gitPath))
                {
                    // Load using LibGit2Sharp
                    using (var gitRepo = new Repository(gitPath, new RepositoryOptions()))
                    {
                        return UpdateOrAdd(gitRepo, false);
                    }
                }

                return null;
            });
        }

        public Task RemoveAll()
        {
            return Task.Run(() =>
            {
                _repositories.Clear();

                // Repository Event (Fetch)
                _eventAggregator.GetEvent<RepositoryEvent>().Publish(RepositoryEventType.RemoveAll);
            });
        }

        // Updates configuration and returns Libgit2Sharp repository object
        private GitRepositoryStub UpdateOrAdd(Repository gitRepo, bool isFetch)
        {
            if (!Directory.Exists(gitRepo.Info.Path))
                throw new Exception("Git repository doesn't exist locally. Please do a clone first");

            GitRepositoryStub repository = null;

            var baseInfo = new DirectoryInfo(gitRepo.Info.Path);
            var gitName = baseInfo.Parent.Name;

            // Initial Creation
            if (!_repositories.Any(x => x.Name == gitName))
            {
                repository = new GitRepositoryStub()
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

                _repositories.Add(repository);

                // Repository Event (Fetch)
                _eventAggregator.GetEvent<RepositoryEvent>().Publish(RepositoryEventType.Add);
            }

            // Already Exists
            else
            {
                repository = _repositories.First(x => x.Name == gitName);

                repository.LastCommitLocal = string.Format("{0}, {1}, {2}",
                                gitRepo.Head.Tip.Author.Name,
                                gitRepo.Head.Tip.Author.Email,
                                gitRepo.Head.Tip.Author.When);
                repository.LastCommitRemote = string.Format("{0}, {1}, {2}",
                                gitRepo.Head.Tip.Author.Name,
                                gitRepo.Head.Tip.Author.Email,
                                gitRepo.Head.Tip.Author.When);

                // Repository Event (Fetch)
                _eventAggregator.GetEvent<RepositoryEvent>().Publish(RepositoryEventType.Load);
            }

            // Fetch time not stored in repository (or I haven't found it yet)
            if (isFetch)
                repository.LastFetch = DateTime.Now;

            return _repositories.First(x => x.Name == gitName);
        }

        public void Dispose()
        {
            _repositories.Clear();

            // Repository Event (Fetch)
            _eventAggregator.GetEvent<RepositoryEvent>().Publish(RepositoryEventType.RemoveAll);
        }
    }
}
