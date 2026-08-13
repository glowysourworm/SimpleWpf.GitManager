using System.Collections.ObjectModel;

using SimpleWpf.ViewModel;

namespace SimpleWpf.GitManager.ViewModel
{
    public class GitManagerLogViewModel : ViewModelBase
    {
        public ObservableCollection<string> Messages { get; set; }

        public GitManagerLogViewModel()
        {
            this.Messages = new ObservableCollection<string>();
        }
    }
}
