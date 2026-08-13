namespace SimpleWpf.GitManager.Model
{
    public class GitRepository
    {
        public string Name { get; set; }
        public string BaseDirectory { get; set; }
        public string GitUrl { get; set; }
        public string LastCommit { get; set; }
        public bool IsFork { get; set; }
        public uint Size { get; set; }
        public DateTimeOffset LastAccessLocal { get; set; }
        public DateTimeOffset LastAccessRemote { get; set; }


        public GitRepository()
        {
            this.BaseDirectory = string.Empty;
            this.GitUrl = string.Empty;
            this.IsFork = false;
            this.Size = 0;
            this.LastAccessLocal = DateTimeOffset.MinValue;
            this.LastAccessRemote = DateTimeOffset.MinValue;
        }
    }
}
