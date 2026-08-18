using System.Collections.ObjectModel;

using SimpleWpf.ViewModel;
using SimpleWpf.Extensions.Collection;

namespace SimpleWpf.GitManager.ViewModel
{
    public class GitManagerViewModel : ViewModelBase
    {
        string _directory;
        string _user;
        string _password;
        int _repositoryCount;
        ObservableCollection<GitManagerRepositoryViewModel> _repositoriesUpToDate;
        ObservableCollection<GitManagerRepositoryViewModel> _repositoriesBehind;
        ObservableCollection<TabViewModel> _tabs;

        bool _loading;

        public string Directory
        {
            get { return _directory; }
            set { this.RaiseAndSetIfChanged(ref _directory, value); }
        }
        public string User
        {
            get { return _user; }
            set { this.RaiseAndSetIfChanged(ref _user, value); }
        }
        public string Password
        {
            get { return _password; }
            set { this.RaiseAndSetIfChanged(ref _password, value); }
        }
        public int RepositoryCount
        {
            get { return _repositoryCount; }
            set { this.RaiseAndSetIfChanged(ref _repositoryCount, value); }
        }
        public ObservableCollection<GitManagerRepositoryViewModel> RepositoriesUpToDate
        {
            get { return _repositoriesUpToDate; }
            set { this.RaiseAndSetIfChanged(ref _repositoriesUpToDate, value); }
        }
        public ObservableCollection<GitManagerRepositoryViewModel> RepositoriesBehind
        {
            get { return _repositoriesBehind; }
            set { this.RaiseAndSetIfChanged(ref _repositoriesBehind, value); }
        }
        public ObservableCollection<TabViewModel> Tabs
        {
            get { return _tabs; }
            set { this.RaiseAndSetIfChanged(ref _tabs, value); }
        }

        public bool Loading
        {
            get { return _loading; }
            set { this.RaiseAndSetIfChanged(ref _loading, value); }
        }

        public bool HasRepository(string name)
        {
            if (this.RepositoriesBehind.Any(x => x.Name == name))
                return true;

            else if (this.RepositoriesUpToDate.Any(x => x.Name == name))
                return true;

            else
                return false;
        }

        public GitManagerRepositoryViewModel GetRepository(string name)
        {
            if (this.RepositoriesBehind.Any(x => x.Name == name))
                return this.RepositoriesBehind.First(x => x.Name == name);

            else if (this.RepositoriesUpToDate.Any(x => x.Name == name))
                return this.RepositoriesUpToDate.First(x => x.Name == name);

            else
                throw new Exception("Repository not found:  " + name);
        }

        public void RemoveRepository(string name)
        {
            IEnumerable<GitManagerRepositoryViewModel> removed = null;

            if (this.RepositoriesBehind.Any(x => x.Name == name))
                removed = this.RepositoriesBehind.Remove(x => x.Name == name);

            else if (this.RepositoriesUpToDate.Any(x => x.Name == name))
                removed = this.RepositoriesUpToDate.Remove(x => x.Name == name);

            else
                throw new Exception("Repository not found:  " + name);

            // Unhook
            foreach (var repo in removed)
            {
                repo.PropertyChanged -= Repository_PropertyChanged;

                // Decrement
                this.RepositoryCount--;
            }
        }

        public void ClearAllRepositories()
        {
            for (int index = this.RepositoriesBehind.Count - 1; index >= 0; index--)
                RemoveRepository(this.RepositoriesBehind[index].Name);

            for (int index = this.RepositoriesUpToDate.Count - 1; index >= 0; index--)
                RemoveRepository(this.RepositoriesUpToDate[index].Name);
        }

        public void AddRepository(GitManagerRepositoryViewModel viewModel)
        {
            if (HasRepository(viewModel.Name))
                throw new Exception("Dupliate repository found:  " + viewModel.Name);

            if (viewModel.IsBehind)
                this.RepositoriesBehind.Add(viewModel);
            else
                this.RepositoriesUpToDate.Add(viewModel);

            // Increment
            this.RepositoryCount++;

            // Hook
            viewModel.PropertyChanged -= Repository_PropertyChanged;
            viewModel.PropertyChanged += Repository_PropertyChanged;
        }

        public GitManagerViewModel()
        {
            this.Directory = string.Empty;
            this.User = string.Empty;
            this.Password = string.Empty;
            this.RepositoryCount = 0;
            this.RepositoriesUpToDate = new ObservableCollection<GitManagerRepositoryViewModel>();
            this.RepositoriesBehind = new ObservableCollection<GitManagerRepositoryViewModel>();
            this.Tabs = new ObservableCollection<TabViewModel>();

            this.Loading = false;
        }

        private void Repository_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var viewModel = sender as GitManagerRepositoryViewModel;

            if (viewModel != null)
            {
                // Just check collections
                if (viewModel.IsBehind && this.RepositoriesUpToDate.Contains(viewModel))
                {
                    this.RepositoriesUpToDate.Remove(viewModel);
                    this.RepositoriesBehind.Add(viewModel);
                }

                else if (!viewModel.IsBehind && this.RepositoriesBehind.Contains(viewModel))
                {
                    this.RepositoriesUpToDate.Add(viewModel);
                    this.RepositoriesBehind.Remove(viewModel);
                }

            }
        }
    }
}
