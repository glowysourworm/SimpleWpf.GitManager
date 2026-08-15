using System.IO;
using System.Xml.Serialization;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.GitManager.Event;
using SimpleWpf.GitManager.Interface;
using SimpleWpf.GitManager.Model;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.SimpleCollections.Collection;
using SimpleWpf.SimpleCollections.Extension;

namespace SimpleWpf.GitManager.Component
{
    [IocExport(typeof(IGitLogManager))]
    public class GitLogManager : IGitLogManager
    {
        readonly string LOG_DIRECTORY = ".\\Logs";
        readonly string LOG_EXT = ".gmlog";

        private readonly IIocEventAggregator _eventAggregator;

        SimpleDictionary<string, GitRepositoryLog> _repositoryLogs;

        [IocImportingConstructor]
        public GitLogManager(IIocEventAggregator eventAggregator)
        {
            _repositoryLogs = new SimpleDictionary<string, GitRepositoryLog>();

            _eventAggregator = eventAggregator;
        }

        public void Initialize(IEnumerable<GitRepositoryStub> repositories)
        {
            if (!Directory.Exists(LOG_DIRECTORY))
                Directory.CreateDirectory(LOG_DIRECTORY);

            _repositoryLogs.Clear();

            var duplicates = repositories.WithDuplicate(x => x.Name);

            if (duplicates.Any())
                throw new Exception("Duplicate repositories found!");

            foreach (var repository in repositories)
            {
                var logFile = CreateLogFilePath(repository.Name);

                // Create / Load
                var log = LoadLog(repository.Name);

                _repositoryLogs.Add(repository.Name, log);
            }
        }

        public bool Exists(string repositoryName)
        {
            return _repositoryLogs.ContainsKey(repositoryName);
        }

        public void Add(string repositoryName)
        {
            if (_repositoryLogs.ContainsKey(repositoryName))
                throw new Exception("IGitLogManager already contains log for specified repository");

            GitRepositoryLog log = null;

            // Check Disk
            if (LogExists(repositoryName))
                log = LoadLog(repositoryName);

            else
                log = new GitRepositoryLog();

            // Add
            _repositoryLogs.Add(repositoryName, log);

            // Save (ensure file on disk)
            SaveLog(repositoryName);
        }

        public void Clear(string repositoryName)
        {
            if (!_repositoryLogs.ContainsKey(repositoryName))
                throw new ArgumentException("Repository not found");

            _repositoryLogs[repositoryName].Messages.Clear();
        }

        public void Remove(string repositoryName)
        {
            RemoveLog(repositoryName);

            _repositoryLogs.Filter(x => x.Key == repositoryName);
        }

        public void RemoveAll()
        {
            foreach (var repository in _repositoryLogs.Keys)
            {
                RemoveLog(repository);
            }

            _repositoryLogs.Clear();
        }

        public GitRepositoryLog Get(string repositoryName)
        {
            if (!_repositoryLogs.ContainsKey(repositoryName))
                throw new ArgumentException("Repository not found");

            return _repositoryLogs[repositoryName];
        }

        public void Log(string repositoryName, string logMessage)
        {
            var log = new GitRepositoryLogData()
            {
                Message = logMessage,
                Timestamp = DateTime.Now
            };

            _repositoryLogs[repositoryName].Messages.Add(log);

            SaveLog(repositoryName);

            _eventAggregator.GetEvent<LogEvent>().Publish(new LogEventData()
            {
                RepositoryName = repositoryName,
                Data = log
            });
        }

        public void Log(string repositoryName, IEnumerable<string> logMessages)
        {
            foreach (var message in logMessages)
            {
                var log = new GitRepositoryLogData()
                {
                    Message = message,
                    Timestamp = DateTime.Now
                };

                _repositoryLogs[repositoryName].Messages.Add(log);

                _eventAggregator.GetEvent<LogEvent>().Publish(new LogEventData()
                {
                    RepositoryName = repositoryName,
                    Data = log
                });
            }

            SaveLog(repositoryName);
        }

        public void RemoveUnused(IEnumerable<GitRepositoryStub> currentList)
        {
            // Log Directory
            var logFiles = Directory.GetFiles(LOG_DIRECTORY, "*" + LOG_EXT);

            // Search for unused log files
            foreach (var file in logFiles)
            {
                var repoName = Path.GetFileName(file);

                if (!currentList.Any(x => x.Name == repoName))
                    File.Delete(file);
            }
        }

        private void RemoveLog(string repositoryName)
        {
            try
            {
                var logPath = CreateLogFilePath(repositoryName);

                if (File.Exists(logPath))
                    File.Delete(logPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void SaveLog(string repositoryName)
        {
            try
            {
                var logPath = CreateLogFilePath(repositoryName);
                var logMessages = _repositoryLogs[repositoryName];

                using (var stream = File.OpenWrite(logPath))
                {
                    var serializer = new XmlSerializer(typeof(GitRepositoryLog));

                    serializer.Serialize(stream, logMessages);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private bool LogExists(string repositoryName)
        {
            try
            {
                var logPath = CreateLogFilePath(repositoryName);

                return File.Exists(logPath);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private GitRepositoryLog LoadLog(string repositoryName)
        {
            try
            {
                var logPath = CreateLogFilePath(repositoryName);

                // New
                if (!File.Exists(logPath))
                    return new GitRepositoryLog();

                var result = new GitRepositoryLog();

                using (var stream = File.OpenRead(logPath))
                {
                    var serializer = new XmlSerializer(typeof(GitRepositoryLog));

                    // Load
                    var log = (GitRepositoryLog)serializer.Deserialize(stream);

                    // OrderBy timestamp
                    foreach (var message in log.Messages.OrderBy(x => x.Timestamp))
                    {
                        result.Messages.Add(message);

                        _eventAggregator.GetEvent<LogEvent>().Publish(new LogEventData()
                        {
                            RepositoryName = repositoryName,
                            Data = message
                        });
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string CreateLogFilePath(string repositoryName)
        {
            return Path.Combine(LOG_DIRECTORY, repositoryName + LOG_EXT);
        }

        public void Dispose()
        {
            _repositoryLogs.Clear();
        }
    }
}
