using System.Windows.Controls;

using SimpleWpf.GitManager.ViewModel;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace SimpleWpf.GitManager.View
{
    [IocExportDefault]
    public partial class RepositoryView : UserControl
    {
        private readonly IIocEventAggregator _eventAggregator;

        GitManagerRepositoryViewModel _viewModel;

        public RepositoryView()
        {
            InitializeComponent();

            this.DataContextChanged += RepositoryView_DataContextChanged;
        }

        [IocImportingConstructor]
        public RepositoryView(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            InitializeComponent();

            this.DataContextChanged += RepositoryView_DataContextChanged;
        }

        private void RepositoryView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
                _viewModel = (e.NewValue as TabViewModel).TabDataContext as GitManagerRepositoryViewModel;
        }
    }
}
