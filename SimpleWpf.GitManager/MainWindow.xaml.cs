using System.ComponentModel;
using System.IO;
using System.Windows;

using Microsoft.Win32;

using SimpleWpf.GitManager.Event;
using SimpleWpf.GitManager.Interface;
using SimpleWpf.GitManager.Model;
using SimpleWpf.GitManager.ViewModel;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.Utilities;

namespace SimpleWpf.GitManager
{
    [IocExportDefault]
    public partial class MainWindow : Window
    {
        readonly IIocEventAggregator _eventAggregator;
        readonly IGitController _controller;
        readonly IDialogController _dialogController;
        readonly IGitLogManager _logManager;

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
            _eventAggregator = eventAggregator;
            _controller = controller;
            _logManager = logManager;
            _dialogController = dialogController;
            _viewModel = new GitManagerViewModel();

            InitializeComponent();

            // Configuration (Loaded)
            eventAggregator.GetEvent<ConfigurationLoadedEvent>().Subscribe(configuration =>
            {
                BasicHelpers.BeginInvokeDispatcher(UpdateConfiguration, System.Windows.Threading.DispatcherPriority.Background, configuration);
            });

            this.DataContext = _viewModel;
        }

        private void UpdateConfiguration(GitManagerConfiguration configuration)
        {
            _viewModel.Directory = configuration.Directory;
            _viewModel.User = configuration.User;
            _viewModel.Password = configuration.Password;
            _viewModel.Repositories.Clear();

            this.PasswordTB.Password = configuration.Password;

            foreach (var repository in configuration.Repositories)
            {
                // Repository
                var repositoryViewModel = new GitManagerRepositoryViewModel()
                {
                    Name = repository.Name,
                    BaseDirectory = repository.BaseDirectory,
                    GitUrl = repository.GitUrl,
                    LastCommitLocal = repository.LastCommitLocal,
                    LastCommitRemote = repository.LastCommitRemote,
                    LastFetch = repository.LastFetch,
                    IsFork = repository.IsFork,
                    Size = repository.Size,
                };

                // Log
                var repositoryLog = _logManager.GetLog(repository.Name);

                foreach (var message in repositoryLog.Messages)
                {
                    repositoryViewModel.Log.Add(message);
                }

                _viewModel.Repositories.Add(repositoryViewModel);
            }

            _eventAggregator.GetEvent<StatusEvent>().Subscribe(message => this.StatusTB.Text = message);

            // Event already sent from initialization
            this.StatusTB.Text = "Configuration Loaded:  " + _controller.GetConfigurationFile();
        }

        private void SetConfiguration()
        {
            _controller.SetConfiguration(configuration =>
            {
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

                    repo.LastCommitLocal = repository.LastCommitLocal;
                    repo.LastCommitRemote = repository.LastCommitRemote;
                    repo.LastFetch = repository.LastFetch;
                    repo.IsFork = repository.IsFork;
                    repo.BaseDirectory = repository.BaseDirectory;
                    repo.Name = repository.Name;
                    repo.Size = repository.Size;
                    repo.GitUrl = repository.GitUrl;
                }
            });
        }


        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // TODO:  Create Bootstrapper Logic
            try
            {
                SetConfiguration();

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

        private async void OpenDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();

            if (dialog.ShowDialog() == true)
            {
                _viewModel.Directory = dialog.FolderName;
                _viewModel.Repositories.Clear();

                _controller.SetConfiguration(configuration =>
                {
                    configuration.Directory = _viewModel.Directory;
                });

                // Re-initialize
                await _controller.RemoveAllReposFromConfiguration();
                await _controller.ReloadAllReposFromConfiguration();
            }
        }

        private void AddRepositoryButton_Click(object sender, RoutedEventArgs e)
        {
            var newRepository = new GitManagerRepositoryViewModel();

            _dialogController.ShowDialogWindowSync(new DialogEventData(newRepository));
        }

        private async void FetchRepositoryButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = this.RepoLB.SelectedItems.Cast<GitManagerRepositoryViewModel>().ToList();

            foreach (GitManagerRepositoryViewModel repository in selectedItems)
            {
                this.StatusTB.Text = "Fetching Repository:  " + repository.GitUrl;

                await _controller.Fetch(Path.Combine(repository.BaseDirectory, ".git"), repository.GitUrl);

                this.StatusTB.Text = "Fetch Complete:  " + repository.GitUrl;
            }
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