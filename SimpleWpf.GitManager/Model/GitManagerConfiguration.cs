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

        public GitManagerConfiguration()
        {

        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(this.Directory))
                return false;

            if (string.IsNullOrWhiteSpace(this.User))
                return false;

            if (string.IsNullOrWhiteSpace(this.Password))
                return false;

            if (!System.IO.Directory.Exists(this.Directory))
                return false;

            return true;
        }
    }
}
