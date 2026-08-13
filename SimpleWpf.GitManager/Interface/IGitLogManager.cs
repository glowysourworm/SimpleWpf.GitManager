using SimpleWpf.GitManager.Model;

namespace SimpleWpf.GitManager.Interface
{
    public interface IGitLogManager
    {
        GitManagerLog GetLog(string repositoryName);

        void SaveLog(string repositoryName, GitManagerLog log);
    }
}
