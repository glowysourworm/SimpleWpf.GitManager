using System.ComponentModel;
using System.Windows;

using SimpleWpf.GitManager.Interface;
using SimpleWpf.GitManager.ViewModel;
using SimpleWpf.IocFramework.Application.Attribute;

namespace SimpleWpf.GitManager
{
    [IocExportDefault]
    public partial class MainWindow : Window
    {
        readonly IGitController _controller;
        readonly string SHUTDOWN_ERROR_MSG = "Error shutting down Git Manager. Shutdown anyway? Your repository data in the configuration may be lost!";

        GitManagerViewModel _viewModel;

        // Designer Constructor
        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new GitManagerViewModel();
        }

        [IocImportingConstructor]
        public MainWindow(IGitController controller, IGitLogManager logManager)
        {
            _controller = controller;
            _viewModel = new GitManagerViewModel();

            InitializeComponent();

            // Read Configuration
            var configuration = controller.GetConfiguration();

            _viewModel.Directory = configuration.Directory;

            foreach (var repository in configuration.Repositories)
            {
                // Repository
                var repositoryViewModel = new GitManagerRepositoryViewModel()
                {
                    Name = repository.Name,
                    BaseDirectory = repository.BaseDirectory,
                    GitUrl = repository.GitUrl,
                    IsFork = repository.IsFork,
                    LastAccessLocal = repository.LastAccessLocal,
                    LastAccessRemote = repository.LastAccessRemote,
                    Password = repository.Password,
                    Size = repository.Size,
                    User = repository.User,
                };

                // Log
                var repositoryLog = logManager.GetLog(repository.Name);

                foreach (var message in repositoryLog.Messages)
                {
                    repositoryViewModel.Log.Add(message);
                }

                _viewModel.Repositories.Add(repositoryViewModel);
            }

            this.DataContext = _viewModel;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // Startup already failed
            if (_controller.GetConfiguration() == null)
            {
                return;
            }

            // TODO:  Create Bootstrapper Logic
            Exception exception = null;
            _controller.Shutdown(out exception);

            // Error
            if (exception != null)
            {
                if (MessageBox.Show(SHUTDOWN_ERROR_MSG, exception.Message, MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                {
                    return;
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void OpenDirectoryButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}