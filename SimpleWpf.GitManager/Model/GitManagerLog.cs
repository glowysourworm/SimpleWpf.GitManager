namespace SimpleWpf.GitManager.Model
{
    public class GitManagerLog
    {
        public List<string> Messages { get; set; }

        public GitManagerLog()
        {
            this.Messages = new List<string>();
        }
    }
}
