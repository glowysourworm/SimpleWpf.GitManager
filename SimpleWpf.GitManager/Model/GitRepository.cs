namespace SimpleWpf.GitManager.Model
{
    public class GitRepository
    {
        public string Name { get; set; }
        public string BaseDirectory { get; set; }
        public string LogFile { get; set; }
        public string GitUrl { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool IsFork { get; set; }
        public uint Size { get; set; }
        public DateTime LastAccessLocal { get; set; }
        public DateTime LastAccessRemote { get; set; }


        public GitRepository()
        {
            this.BaseDirectory = string.Empty;
            this.GitUrl = string.Empty;
            this.LogFile = string.Empty;
            this.User = string.Empty;
            this.Password = string.Empty;
            this.IsFork = false;
            this.Size = 0;
            this.LastAccessLocal = DateTime.MinValue;
            this.LastAccessRemote = DateTime.MinValue;
        }
    }
}
