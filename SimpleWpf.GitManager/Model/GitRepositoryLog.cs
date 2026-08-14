namespace SimpleWpf.GitManager.Model
{
    public class GitRepositoryLog
    {
        public List<GitRepositoryLogData> Messages { get; set; }

        public GitRepositoryLog()
        {
            this.Messages = new List<GitRepositoryLogData>();
        }
    }
}
