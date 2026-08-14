using System.IO;

using SimpleWpf.GitManager.Interface;
using SimpleWpf.GitManager.Model;
using SimpleWpf.IocFramework.Application.Attribute;

namespace SimpleWpf.GitManager.Component
{
    [IocExport(typeof(IGitLogManager))]
    public class GitLogManager : IGitLogManager
    {
        readonly string LOG_DIRECTORY = ".\\";

        public GitLogManager()
        {

        }

        public GitManagerLog GetLog(string repositoryName)
        {
            try
            {
                var logPath = Path.Combine(LOG_DIRECTORY, repositoryName, ".txt");

                // New Log
                if (!File.Exists(logPath))
                    return new GitManagerLog();

                var logFile = File.ReadAllText(logPath);

                var logs = logFile.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var result = new GitManagerLog();

                foreach (var log in logs)
                    result.Messages.Add(log);

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading repository log", ex);
            }
        }

        public void RemoveLog(string repositoryName)
        {
            try
            {
                var logPath = Path.Combine(LOG_DIRECTORY, repositoryName, ".txt");

                if (File.Exists(logPath))
                    File.Delete(logPath);
            }
            catch (Exception ex)
            {
                throw new Exception("Error removing repository log", ex);
            }
        }

        public void SaveLog(string repositoryName, GitManagerLog log)
        {
            try
            {
                var logPath = Path.Combine(LOG_DIRECTORY, repositoryName, ".txt");

                using (var stream = File.OpenWrite(logPath))
                {
                    using (var writer = new StreamWriter(stream))
                    {
                        foreach (var message in log.Messages)
                            writer.WriteLine(message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error writing repository log", ex);
            }
        }
    }
}
