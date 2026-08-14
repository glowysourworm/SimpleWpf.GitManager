using System.Windows.Controls;

using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace SimpleWpf.GitManager.View
{
    [IocExportDefault]
    public partial class RepositoryView : UserControl
    {
        private readonly IIocEventAggregator _eventAggregator;

        public RepositoryView()
        {
            InitializeComponent();
        }

        [IocImportingConstructor]
        public RepositoryView(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            InitializeComponent();
        }
    }
}
