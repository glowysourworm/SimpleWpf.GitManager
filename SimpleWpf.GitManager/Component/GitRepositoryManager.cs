using System.IO;

using SimpleGit;

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

        private List<GitRepository> _repositories;

        [IocImportingConstructor]
        public GitRepositoryManager(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _repositories = new List<GitRepository>();
        }

        public GitRepository Get(string gitName)
        {
            return _repositories.First(x => x.Name == gitName);
        }

        public IEnumerable<GitRepository> GetAll()
        {
            return _repositories;
        }

        public IEnumerable<string> GetList()
        {
            return _repositories.Select(r => r.Name).ToList();
        }

        public void Fetch(string gitName, string userName, string password, GitHandlers.GitLogHandler logHandler)
        {
            try
            {
                var repository = _repositories.First(x => x.Name == gitName);

                // Update Fetch
                //repository.LastFetch = DateTime.Now;

                var proxy = new GitProxy(repository);

                // -> SimpleGit Fetch (command line terminal proxy)
                proxy.Fetch(logHandler);

                _eventAggregator.GetEvent<RepositoryEvent>().Publish(new RepositoryEventData()
                {
                    EventType = RepositoryEventType.Fetch,
                    RepositoryName = gitName
                });
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
                UpdateOrAdd(gitPath);
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
        private GitRepository UpdateOrAdd(string gitPath)
        {
            if (!Directory.Exists(gitPath))
                throw new Exception("Git repository doesn't exist locally. Please do a clone first");

            var repository = GitRepository.Load(gitPath);

            // Initial Creation
            if (!_repositories.Any(x => x.Name == repository.Name))
            {
                _repositories.Add(repository);

                // Repository Event (Add)
                _eventAggregator.GetEvent<RepositoryEvent>().Publish(new RepositoryEventData()
                {
                    EventType = RepositoryEventType.Add,
                    RepositoryName = repository.Name
                });
            }

            // Already Exists
            else
            {
                var existing = _repositories.First(x => x.Name == repository.Name);

                existing.LastCommitLocal = repository.LastCommitLocal;
                existing.LastCommitRemote = repository.LastCommitRemote;

                // Repository Event (Load)
                _eventAggregator.GetEvent<RepositoryEvent>().Publish(new RepositoryEventData()
                {
                    EventType = RepositoryEventType.Load,
                    RepositoryName = repository.Name
                });
            }

            return _repositories.First(x => x.Name == repository.Name);
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
