namespace SimpleWpf.GitManager.Model
{
    public class GitRepository
    {
        public string Name { get; set; }
        public string BaseDirectory { get; set; }
        public string GitUrl { get; set; }
        public string LastCommitLocal { get; set; }
        public string LastCommitRemote { get; set; }
        public DateTimeOffset LastFetch { get; set; }
        public bool IsFork { get; set; }
        public bool IsHeadUpToDate { get; set; }
        public uint Size { get; set; }

        public GitRepository()
        {
            this.BaseDirectory = string.Empty;
            this.GitUrl = string.Empty;
            this.IsFork = false;
            this.IsHeadUpToDate = false;
            this.Size = 0;
            this.LastCommitLocal = string.Empty;
            this.LastCommitRemote = string.Empty;
            this.LastFetch = DateTimeOffset.MinValue;
        }
    }
}
