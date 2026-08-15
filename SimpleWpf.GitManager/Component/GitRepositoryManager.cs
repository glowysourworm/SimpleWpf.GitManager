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

        private List<GitRepositoryStub> _repositories;

        [IocImportingConstructor]
        public GitRepositoryManager(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _repositories = new List<GitRepositoryStub>();
        }

        public GitRepositoryStub Get(string gitName)
        {
            return _repositories.First(x => x.Name == gitName);
        }

        public IEnumerable<GitRepositoryStub> GetAll()
        {
            return _repositories;
        }

        public IEnumerable<string> GetList()
        {
            return _repositories.Select(r => r.Name).ToList();
        }

        public void Fetch(string gitName, ProgressHandler progressHandler)
        {
            try
            {
                var repository = _repositories.First(x => x.Name == gitName);

                using (var gitRepo = new Repository(repository.GitPath))
                {
                    // Repository Event (Fetch)
                    var baseInfo = new DirectoryInfo(gitRepo.Info.Path);

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

                    Commands.Fetch(gitRepo, gitRepo.Head.RemoteName, Enumerable.Empty<string>(), new FetchOptions()
                    {
                        CredentialsProvider = credentialsCallback,
                        Prune = false,
                        OnTransferProgress = progressCallback,
                        OnProgress = progressHandler

                    }, "Fetching from GitManager:  " + DateTime.Now.ToString());

                    _eventAggregator.GetEvent<RepositoryEvent>().Publish(new RepositoryEventData()
                    {
                        EventType = RepositoryEventType.Fetch,
                        RepositoryName = gitName
                    });

                    UpdateOrAdd(gitRepo, true);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Initialize(GitManagerConfiguration configuration)
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

                        Load(gitPath);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Load(string gitPath)
        {
            if (Directory.Exists(gitPath))
            {
                // Load using LibGit2Sharp
                using (var gitRepo = new Repository(gitPath, new RepositoryOptions()))
                {
                    UpdateOrAdd(gitRepo, false);
                }
            }
        }

        public void RemoveAll()
        {
            _repositories.Clear();

            // Repository Event (Fetch)
            _eventAggregator.GetEvent<RepositoryEvent>().Publish(new RepositoryEventData()
            {
                EventType = RepositoryEventType.RemoveAll,
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
                    LastFetch = baseInfo.LastAccessTime,
                    LastCommitLocal = string.Format("{0}, {1}",
                                    gitRepo.Head.Tip.Author.Name,
                                    gitRepo.Head.Tip.Author.When.ToString("yyyy-MM-dd hh:mm:ss tt")),
                    LastCommitRemote = string.Format("{0}, {1}",
                                    gitRepo.Head.Tip.Author.Name,
                                    gitRepo.Head.Tip.Author.When.ToString("yyyy-MM-dd hh:mm:ss tt")),
                    GitUrl = gitRepo.Network.Remotes.FirstOrDefault()?.Url ?? "Not Specified",
                    GitPath = gitRepo.Info.Path
                };

                _repositories.Add(repository);

                // Repository Event (Add)
                _eventAggregator.GetEvent<RepositoryEvent>().Publish(new RepositoryEventData()
                {
                    EventType = RepositoryEventType.Add,
                    RepositoryName = gitName
                });
            }

            // Already Exists
            else
            {
                repository = _repositories.First(x => x.Name == gitName);

                repository.LastCommitLocal = string.Format("{0}, {1}",
                                gitRepo.Head.Tip.Author.Name,
                                gitRepo.Head.Tip.Author.When.ToString("yyyy-MM-dd hh:mm:ss tt"));
                repository.LastCommitRemote = string.Format("{0}, {1}",
                                gitRepo.Head.Tip.Author.Name,
                                gitRepo.Head.Tip.Author.When.ToString("yyyy-MM-dd hh:mm:ss tt"));
                repository.LastFetch = baseInfo.LastAccessTime;

                // Repository Event (Load)
                _eventAggregator.GetEvent<RepositoryEvent>().Publish(new RepositoryEventData()
                {
                    EventType = RepositoryEventType.Load,
                    RepositoryName = gitName
                });
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
            _eventAggregator.GetEvent<RepositoryEvent>().Publish(new RepositoryEventData()
            {
                EventType = RepositoryEventType.RemoveAll
            });
        }
    }
}
