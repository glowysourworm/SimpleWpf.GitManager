using SimpleGit.Component;
using SimpleGit.Model;

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

        public IEnumerable<GitRepositoryStub> GetList()
        {
            return _repositories;
        }

        public Task Clone(GitManagerConfiguration configuration, GitRepositoryStub repository, GitHandlers.GitLogHandler logHandler)
        {
            return Task.Run(async () =>
            {
                using (var proxy = new GitProxy())
                {
                    try
                    {
                        // -> SimpleGit Clone
                        //
                        var response = await proxy.Process(new GitRepositoryRequest()
                        {
                            BaseDirectory = repository.BaseDirectory,
                            LogHandler = logHandler,
                            User = configuration.User,
                            Password = configuration.Password,
                            RepositoryName = repository.Name,
                            Type = GitRepositoryRequest.RequestType.Clone,
                            Url = repository.Url
                        });

                        _eventAggregator.GetEvent<RepositoryEvent>().Publish(new RepositoryEventData()
                        {
                            EventType = RepositoryEventType.Clone,
                            RepositoryName = repository.Name
                        });
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error cloning git repository", ex);
                    }
                }
            });
        }

        public Task Fetch(GitManagerConfiguration configuration, GitRepositoryStub repository, GitHandlers.GitLogHandler logHandler)
        {
            return Task.Run(async () =>
            {
                using (var proxy = new GitProxy())
                {
                    try
                    {
                        // -> SimpleGit Fetch
                        //
                        var response = await proxy.Process(new GitRepositoryRequest()
                        {
                            BaseDirectory = repository.BaseDirectory,
                            WorkingDirectory = repository.WorkingDirectory,
                            LogHandler = logHandler,
                            User = configuration.User,
                            Password = configuration.Password,
                            RepositoryName = repository.Name,
                            Type = GitRepositoryRequest.RequestType.Clone,
                            Url = repository.Url
                        });

                        _eventAggregator.GetEvent<RepositoryEvent>().Publish(new RepositoryEventData()
                        {
                            EventType = RepositoryEventType.Fetch,
                            RepositoryName = repository.Name
                        });
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Error fetching git repository", ex);
                    }
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
                        using (var proxy = new GitProxy())
                        {
                            var response = await proxy.Process(new GitRepositoryRequest()
                            {
                                BaseDirectory = configuration.Directory,
                                Password = configuration.Password,
                                User = configuration.User,
                                Type = GitRepositoryRequest.RequestType.LocalReadAll,
                                LogHandler = (message) => { return true; }
                            });

                            foreach (var data in response.MultipleResponseData)
                            {
                                if (!ValidateResponseData(data))
                                    throw new Exception("Invalid GitProxy response");

                                var repository = new GitRepositoryStub(data);

                                UpdateOrAdd(repository);
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

        public Task ReInitialize(GitManagerConfiguration configuration)
        {
            _repositories.Clear();

            // Repository Event (RemoveAll)
            _eventAggregator.GetEvent<RepositoryEvent>().Publish(new RepositoryEventData()
            {
                EventType = RepositoryEventType.RemoveAll,
            });

            return Initialize(configuration);
        }

        private bool ValidateResponseData(GitResponseData data)
        {
            if (data.Local == null &&
                data.Remote == null)
                return false;

            if (data.Local?.Name == null &&
                data.Remote?.Name == null)
                return false;

            // Local
            if (data.Local != null)
            {
                if (string.IsNullOrWhiteSpace(data.Local.WorkingDirectory))
                    return false;
            }

            // Remote
            if (data.Remote != null)
            {
                if (string.IsNullOrWhiteSpace(data.Remote.Url))
                    return false;
            }

            // Local | Remote
            if (data.Remote != null && data.Local != null)
            {
                if (data.Status == null)
                    return false;
            }

            return true;
        }

        // Updates configuration and returns Libgit2Sharp repository object
        private void UpdateOrAdd(GitRepositoryStub repository)
        {
            // Add
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

            // Update
            else
            {
                var existing = _repositories.First(x => x.Name == repository.Name);

                // Update
                existing.Update(repository);

                // Repository Event (Load)
                _eventAggregator.GetEvent<RepositoryEvent>().Publish(new RepositoryEventData()
                {
                    EventType = RepositoryEventType.Update,
                    RepositoryName = repository.Name
                });
            }
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
