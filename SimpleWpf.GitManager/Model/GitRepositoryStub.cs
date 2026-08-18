using SimpleGit.Model;

namespace SimpleWpf.GitManager.Model
{
    public class GitRepositoryStub
    {
        public long Id { get; private set; }
        public string Name { get; private set; }
        public string OwnerName { get; private set; }
        public string BaseDirectory { get; private set; }
        public string WorkingDirectory { get; private set; }
        public string Url { get; private set; }
        public GitCommit? LastCommitLocal { get; set; }
        public GitCommit? LastCommitRemote { get; set; }
        public long RemoteSize { get; set; }
        public int BehindBy { get; set; }
        public int AheadBy { get; set; }
        public bool IsBehind { get; set; }
        public bool IsAhead { get; set; }
        public bool IsFork { get; set; }

        /// <summary>
        /// Repository needs to be cloned locally
        /// </summary>
        public bool IsRemoteOnly { get; set; }

        /// <summary>
        /// Repository has no remote and is just local
        /// </summary>
        public bool IsLocalOnly { get; set; }

        /// <summary>
        /// Repository has a local clone, with a remote
        /// </summary>
        public bool IsLocalClone { get; set; }

        public void SetFromResponse(GitRepositoryResponse response)
        {
            if (response.Local?.Id == null &&
                response.Remote?.Id == null)
                throw new ArgumentException("Invalid git repository response:  must have valid id");

            if (response.Local?.Name == null &&
                response.Remote?.Name == null)
                throw new ArgumentException("Invalid git repository response:  must have valid name");

            this.Id = response.Local?.Id ?? response.Remote?.Id ?? 0;
            this.Name = response.Local?.Name ?? response.Remote?.Name ?? string.Empty;

            // Local
            if (response.Local != null)
            {
                var head = response.Local.GetHead();

                this.WorkingDirectory = response.Local.WorkingDirectory;
                this.Url = response.Local.Remotes.FirstOrDefault(x => x.Name == head.RemoteName)?.Url ?? string.Empty;
                this.LastCommitLocal = head.LastCommit;

                this.IsLocalClone = response.Remote != null;
                this.IsLocalOnly = response.Remote == null;
            }

            // Remote
            if (response.Remote != null)
            {
                var head = response.Remote.GetHead();

                this.Url = response.Remote.Url;
                this.LastCommitRemote = head.LastCommit;
                this.RemoteSize = response.Remote.Size;
                this.OwnerName = response.Remote.OwnerName;
                this.IsFork = response.Remote.IsFork;

                this.IsRemoteOnly = response.Local == null;
            }

            // Local | Remove
            if (response.Local != null &&
                response.Remote != null)
            {
                if (response.Status == null)
                    throw new Exception("Commit status was not set from the GitProxy!");

                this.BehindBy = response.Status.IsBehind ? response.Status.CommitDelta : 0;
                this.AheadBy = response.Status.IsAhead ? response.Status.CommitDelta : 0;
                this.IsBehind = response.Status.IsBehind;
                this.IsAhead = response.Status.IsAhead;
            }
        }

        public bool Validate(GitRepositoryStub repository)
        {
            if (this.Id <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(repository.Name))
                return false;

            if (string.IsNullOrWhiteSpace(repository.OwnerName))
                return false;

            if (string.IsNullOrWhiteSpace(repository.BaseDirectory))
                return false;

            if (!repository.IsLocalOnly && string.IsNullOrWhiteSpace(repository.Url))
                return false;

            if (!repository.IsRemoteOnly && string.IsNullOrWhiteSpace(repository.BaseDirectory))
                return false;

            if (!repository.IsRemoteOnly && string.IsNullOrWhiteSpace(repository.WorkingDirectory))
                return false;

            return true;
        }

        public void Update(GitRepositoryStub repository)
        {
            if (!Validate(repository))
                throw new Exception("Invalid GitRepositoryStub");

            this.Id = repository.Id;
            this.Name = repository.Name;
            this.BaseDirectory = repository.BaseDirectory;
            this.WorkingDirectory = repository.WorkingDirectory;
            this.Url = repository.Url;

            this.OwnerName = repository.OwnerName;

            this.LastCommitLocal = repository.LastCommitLocal;
            this.LastCommitRemote = repository.LastCommitRemote;
            this.BehindBy = repository.BehindBy;
            this.AheadBy = repository.AheadBy;
            this.IsBehind = repository.IsBehind;
            this.IsAhead = repository.IsAhead;
            this.IsFork = repository.IsFork;
        }

        public GitRepositoryStub(long id, string name, string baseDirectory, string workingDirectory, string url)
        {
            this.Id = id;
            this.Name = name;
            this.BaseDirectory = baseDirectory;
            this.WorkingDirectory = workingDirectory;
            this.Url = url;

            this.OwnerName = string.Empty;

            this.LastCommitLocal = new GitCommit();
            this.LastCommitRemote = new GitCommit();
            this.BehindBy = 0;
            this.AheadBy = 0;
            this.IsBehind = false;
            this.IsAhead = false;
            this.IsFork = false;
        }

        public GitRepositoryStub(GitRepositoryResponse response)
        {
            SetFromResponse(response);
        }
    }
}
