using SimpleWpf.ViewModel;

namespace SimpleWpf.GitManager.ViewModel
{
    public enum TabType
    {
        Configuration,
        Repository
    }

    public class TabViewModel : ViewModelBase
    {
        string _header;
        TabType _type;
        object? _tabDataContext;

        public string Header
        {
            get { return _header; }
            set { this.RaiseAndSetIfChanged(ref _header, value); }
        }
        public TabType Type
        {
            get { return _type; }
            set { this.RaiseAndSetIfChanged(ref _type, value); }
        }
        public object? TabDataContext
        {
            get { return _tabDataContext; }
            set { this.RaiseAndSetIfChanged(ref _tabDataContext, value); }
        }

        public TabViewModel()
        {
            this.Header = string.Empty;
            this.Type = TabType.Configuration;
            this.TabDataContext = null;
        }
    }
}
