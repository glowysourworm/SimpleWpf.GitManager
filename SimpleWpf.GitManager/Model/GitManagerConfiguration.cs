namespace SimpleWpf.GitManager.Model
{
    public class GitManagerConfiguration
    {
        /// <summary>
        /// Directory of Git repositories to manage
        /// </summary>
        public string Directory { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        /// <summary>
        /// List of repositories under management
        /// </summary>
        public List<GitRepository> Repositories { get; set; }

        public GitManagerConfiguration()
        {
            this.Repositories = new List<GitRepository>();
        }
    }
}
