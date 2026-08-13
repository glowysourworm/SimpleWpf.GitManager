using System.Collections.ObjectModel;

using SimpleWpf.ViewModel;

namespace SimpleWpf.GitManager.ViewModel
{
    public class GitManagerViewModel : ViewModelBase
    {
        string _directory;
        ObservableCollection<GitManagerRepositoryViewModel> _repositories;

        public string Directory
        {
            get { return _directory; }
            set { this.RaiseAndSetIfChanged(ref _directory, value); }
        }
        public ObservableCollection<GitManagerRepositoryViewModel> Repositories
        {
            get { return _repositories; }
            set { this.RaiseAndSetIfChanged(ref _repositories, value); }
        }

        public GitManagerViewModel()
        {
            this.Directory = string.Empty;
            this.Repositories = new ObservableCollection<GitManagerRepositoryViewModel>();
        }
    }
}
