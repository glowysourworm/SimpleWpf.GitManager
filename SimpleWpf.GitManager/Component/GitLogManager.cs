using System.IO;
using System.Xml.Serialization;

using SimpleWpf.Extensions.Collection;
using SimpleWpf.GitManager.Interface;
using SimpleWpf.GitManager.Model;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.SimpleCollections.Collection;
using SimpleWpf.SimpleCollections.Extension;

namespace SimpleWpf.GitManager.Component
{
    [IocExport(typeof(IGitLogManager))]
    public class GitLogManager : IGitLogManager
    {
        readonly string LOG_DIRECTORY = ".\\Logs";
        readonly string LOG_EXT = ".gmlog";

        SimpleDictionary<string, GitRepositoryLog> _repositoryLogs;

        public GitLogManager()
        {
            _repositoryLogs = new SimpleDictionary<string, GitRepositoryLog>();
        }

        public Task Initialize(GitManagerConfiguration configuration)
        {
            return Task.Run(() =>
            {
                if (!Directory.Exists(LOG_DIRECTORY))
                    Directory.CreateDirectory(LOG_DIRECTORY);

                _repositoryLogs.Clear();

                foreach (var repository in configuration.Repositories)
                {
                    var logFile = CreateLogFilePath(repository.BaseDirectory, repository.Name);

                    // Create / Load
                    var log = LoadLog(repository.BaseDirectory, repository.Name);

                    _repositoryLogs.Add(repository.Name, log);
                }
            });
        }

        public void Clear(string repositoryName)
        {
            if (!_repositoryLogs.ContainsKey(repositoryName))
                throw new ArgumentException("Repository not found");

            _repositoryLogs[repositoryName].Messages.Clear();
        }

        public Task Remove(string repositoryName)
        {
            return Task.Run(() =>
            {
                RemoveLog(repositoryName);

                _repositoryLogs.Filter(x => x.Key == repositoryName);
            });
        }

        public Task RemoveAll()
        {
            return Task.Run(() =>
            {
                foreach (var repository in _repositoryLogs.Keys)
                {
                    RemoveLog(repository);
                }

                _repositoryLogs.Clear();
            });
        }

        public GitRepositoryLog Get(string repositoryName)
        {
            if (!_repositoryLogs.ContainsKey(repositoryName))
                throw new ArgumentException("Repository not found");

            return _repositoryLogs[repositoryName];
        }

        public Task Log(string repositoryName, string logMessage)
        {
            return Task.Run(() =>
            {
                _repositoryLogs[repositoryName].Messages.Add(new GitRepositoryLogData()
                {
                    Message = logMessage,
                    Timestamp = DateTime.Now
                });

                SaveLog(repositoryName);
            });
        }

        public Task Log(string repositoryName, IEnumerable<string> logMessages)
        {
            return Task.Run(() =>
            {
                foreach (var message in logMessages)
                    _repositoryLogs[repositoryName].Messages.Add(new GitRepositoryLogData()
                    {
                        Message = message,
                        Timestamp = DateTime.Now
                    });

                SaveLog(repositoryName);
            });
        }

        public Task RemoveUnused(GitManagerConfiguration configuration)
        {
            return Task.Run(() =>
            {
                // Log Directory
                var logFiles = Directory.GetFiles(LOG_DIRECTORY, "*" + LOG_EXT);

                // Search for unused log files
                foreach (var file in logFiles)
                {
                    var repoName = Path.GetFileName(file);

                    if (!configuration.Repositories.Any(x => x.Name == repoName))
                        File.Delete(file);
                }
            });
        }

        private void RemoveLog(string repositoryName)
        {
            try
            {
                var logPath = Path.Combine(LOG_DIRECTORY, repositoryName, LOG_EXT);

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
                var logPath = Path.Combine(LOG_DIRECTORY, repositoryName, LOG_EXT);
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

        private GitRepositoryLog LoadLog(string repositoryDirectory, string repositoryName)
        {
            try
            {
                var logPath = CreateLogFilePath(repositoryDirectory, repositoryName);

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
                        result.Messages.Add(message);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string CreateLogFilePath(string repositoryDirectory, string repositoryName)
        {
            return Path.Combine(LOG_DIRECTORY, repositoryName, LOG_EXT);
        }

        public void Dispose()
        {
            _repositoryLogs.Clear();
        }
    }
}
