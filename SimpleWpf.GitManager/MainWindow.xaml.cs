using System.ComponentModel;
using System.Windows;

using Microsoft.Win32;

using SimpleWpf.GitManager.Event;
using SimpleWpf.GitManager.Interface;
using SimpleWpf.GitManager.Model;
using SimpleWpf.GitManager.ViewModel;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace SimpleWpf.GitManager
{
    [IocExportDefault]
    public partial class MainWindow : Window
    {
        readonly IGitController _controller;
        readonly IDialogController _dialogController;

        readonly string SHUTDOWN_ERROR_MSG = "Error shutting down Git Manager. Shutdown anyway? Your repository data in the configuration may be lost!";

        GitManagerViewModel _viewModel;

        // Designer Constructor
        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new GitManagerViewModel();
        }

        [IocImportingConstructor]
        public MainWindow(IIocEventAggregator eventAggregator,
                          IGitController controller,
                          IDialogController dialogController,
                          IGitLogManager logManager)
        {
            _controller = controller;
            _dialogController = dialogController;
            _viewModel = new GitManagerViewModel();

            InitializeComponent();

            // Read Configuration
            var configuration = controller.GetConfiguration();

            _viewModel.Directory = configuration.Directory;
            _viewModel.User = configuration.User;
            _viewModel.Password = configuration.Password;

            this.PasswordTB.Password = configuration.Password;

            foreach (var repository in configuration.Repositories)
            {
                // Repository
                var repositoryViewModel = new GitManagerRepositoryViewModel()
                {
                    Name = repository.Name,
                    BaseDirectory = repository.BaseDirectory,
                    GitUrl = repository.GitUrl,
                    LastCommit = repository.LastCommit,
                    IsFork = repository.IsFork,
                    LastAccessLocal = repository.LastAccessLocal,
                    LastAccessRemote = repository.LastAccessRemote,
                    Size = repository.Size,
                };

                // Log
                var repositoryLog = logManager.GetLog(repository.Name);

                foreach (var message in repositoryLog.Messages)
                {
                    repositoryViewModel.Log.Add(message);
                }

                _viewModel.Repositories.Add(repositoryViewModel);
            }

            eventAggregator.GetEvent<StatusEvent>().Subscribe(message => this.StatusTB.Text = message);

            // Event already sent from initialization
            this.StatusTB.Text = "Configuration Loaded:  " + _controller.GetConfigurationFile();

            this.DataContext = _viewModel;
        }

        private void WriteConfiguration()
        {
            var configuration = _controller.GetConfiguration();

            configuration.Directory = _viewModel.Directory;
            configuration.User = _viewModel.User;
            configuration.Password = _viewModel.Password;

            foreach (var repository in _viewModel.Repositories)
            {
                var repo = configuration.Repositories.FirstOrDefault(x => x.Name == repository.Name);

                if (repo == null)
                {
                    repo = new GitRepository();

                    configuration.Repositories.Add(repo);
                }

                repo.LastAccessLocal = repository.LastAccessLocal;
                repo.LastAccessRemote = repository.LastAccessRemote;
                repo.LastCommit = repository.LastCommit;
                repo.IsFork = repository.IsFork;
                repo.BaseDirectory = repository.BaseDirectory;
                repo.Name = repository.Name;
                repo.Size = repository.Size;
                repo.GitUrl = repository.GitUrl;
            }
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
            try
            {
                WriteConfiguration();

                _controller.Shutdown();
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(SHUTDOWN_ERROR_MSG, ex.Message, MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
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
            var dialog = new OpenFolderDialog();

            if (dialog.ShowDialog() == true)
            {
                _viewModel.Directory = dialog.FolderName;
            }
        }

        private void AddRepositoryButton_Click(object sender, RoutedEventArgs e)
        {
            var newRepository = new GitManagerRepositoryViewModel();

            _dialogController.ShowDialogWindowSync(new DialogEventData(newRepository));
        }

        private void UpdateRepositoryButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RunScriptButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PasswordTB_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Password = this.PasswordTB.Password;
        }
    }
}