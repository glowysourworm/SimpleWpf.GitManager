namespace SimpleWpf.GitManager.Model
{
    public class GitRepositoryStub
    {
        public string Name { get; set; }
        public string BaseDirectory { get; set; }
        public string GitUrl { get; set; }
        public string GitPath { get; set; }
        public string LastCommitLocal { get; set; }
        public string LastCommitRemote { get; set; }
        public DateTimeOffset LastFetch { get; set; }
        public bool IsFork { get; set; }
        public bool IsHeadUpToDate { get; set; }
        public uint Size { get; set; }

        public GitRepositoryStub()
        {
            this.BaseDirectory = string.Empty;
            this.GitUrl = string.Empty;
            this.GitPath = string.Empty;
            this.IsFork = false;
            this.IsHeadUpToDate = false;
            this.Size = 0;
            this.LastCommitLocal = string.Empty;
            this.LastCommitRemote = string.Empty;
            this.LastFetch = DateTimeOffset.MinValue;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
