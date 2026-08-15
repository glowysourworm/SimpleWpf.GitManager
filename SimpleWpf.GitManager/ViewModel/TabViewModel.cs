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
        bool _isSelected;
        bool _isClosable;
        TabType _type;
        object? _tabDataContext;

        public string Header
        {
            get { return _header; }
            set { this.RaiseAndSetIfChanged(ref _header, value); }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set { this.RaiseAndSetIfChanged(ref _isSelected, value); }
        }
        public bool IsClosable
        {
            get { return _isClosable; }
            set { this.RaiseAndSetIfChanged(ref _isClosable, value); }
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
            this.IsSelected = false;
            this.IsClosable = false;
            this.Type = TabType.Configuration;
            this.TabDataContext = null;
        }
    }
}
