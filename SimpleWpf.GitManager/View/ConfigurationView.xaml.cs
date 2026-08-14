using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;

using SimpleWpf.GitManager.Event;
using SimpleWpf.GitManager.ViewModel;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace SimpleWpf.GitManager.View
{
    [IocExportDefault]
    public partial class ConfigurationView : UserControl
    {
        private readonly IIocEventAggregator _eventAggregator;

        GitManagerViewModel _viewModel;

        public ConfigurationView()
        {
            InitializeComponent();

            this.DataContextChanged += ConfigurationView_DataContextChanged;
        }

        [IocImportingConstructor]
        public ConfigurationView(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            InitializeComponent();

            this.DataContextChanged += ConfigurationView_DataContextChanged;
        }

        private void ConfigurationView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = (e.NewValue as TabViewModel).TabDataContext as GitManagerViewModel;
        }

        private void PasswordTB_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            _viewModel.Password = this.PasswordTB.Password;

            _eventAggregator.GetEvent<ViewEvent>().Publish(new ViewEventData()
            {
                Type = ViewEventType.ConfigurationModified
            });
        }

        private async void OpenDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();

            if (dialog.ShowDialog() == true)
            {
                // Primary ViewModel Binding (Directory)
                _viewModel.Directory = dialog.FolderName;

                _eventAggregator.GetEvent<ViewEvent>().Publish(new ViewEventData()
                {
                    Type = ViewEventType.ConfigurationModifiedReload
                });
            }
        }
    }
}
