using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

using SimpleWpf.Extensions.Collection;
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
            _dialogController = dialogController;
            _logManager = logManager;
            _viewModel = new GitManagerViewModel();

            // Tabs
            _viewModel.Tabs.Add(new TabViewModel()
            {
                Header = "Configuration",
                TabDataContext = _viewModel,
                Type = TabType.Configuration
            });

            InitializeComponent();

            // Configuration (Loaded)
            eventAggregator.GetEvent<ConfigurationEvent>().Subscribe(configuration =>
            {
                BasicHelpers.BeginInvokeDispatcher(OnConfigurationChanged, DispatcherPriority.Background, configuration);
            });

            eventAggregator.GetEvent<LogEvent>().Subscribe(log =>
            {
                BasicHelpers.BeginInvokeDispatcher(OnLog, DispatcherPriority.Background, log);
            });

            eventAggregator.GetEvent<ViewEvent>().Subscribe(data =>
            {
                BasicHelpers.BeginInvokeDispatcher(OnViewEvent, DispatcherPriority.Background, data);
            });

            eventAggregator.GetEvent<RepositoryEvent>().Subscribe(type =>
            {
                BasicHelpers.BeginInvokeDispatcher(OnRepositoryEvent, DispatcherPriority.Background, type);
            });

            this.DataContext = _viewModel;
        }

        private void OnConfigurationChanged(ConfigurationEventType eventType)
        {
            switch (eventType)
            {
                case ConfigurationEventType.Loaded:
                    FromConfiguration();
                    break;
                case ConfigurationEventType.Saved:
                    return;
                case ConfigurationEventType.Modified:
                    FromConfiguration();
                    break;
                default:
                    throw new Exception("Unhandled configuration event type");
            }
        }

        private void OnLog(LogEventData eventData)
        {
            var repository = _viewModel.Repositories.FirstOrDefault(x => x.Name == eventData.RepositoryName);

            if (repository != null)
            {
                repository.Log.Add(new GitManagerLogMessageViewModel()
                {
                    Timestamp = eventData.Data.Timestamp,
                    Message = eventData.Data.Message,
                });
            }
        }

        private async void OnViewEvent(ViewEventData eventData)
        {
            switch (eventData.Type)
            {
                case ViewEventType.ConfigurationModified:
                    break;
                case ViewEventType.ConfigurationModifiedReload:
                {
                    // Clear Repositories
                    _viewModel.Repositories.Clear();

                    // Remove Repository Tabs
                    _viewModel.Tabs.Remove(x => x.Type == TabType.Repository);

                    await _controller.SetConfiguration(configuration =>
                    {
                        configuration.Directory = _viewModel.Directory;
                    });

                    // Re-initialize
                    await _controller.RemoveAllReposFromConfiguration();
                    await _controller.ReloadAllReposFromConfiguration();
                }
                break;
                default:
                    throw new Exception("Unhandled view event type");
            }
        }

        private void OnRepositoryEvent(RepositoryEventType type)
        {
            switch (type)
            {
                case RepositoryEventType.Add:
                    // Clear
                    _viewModel.Repositories.Clear();

                    // Reload
                    FromConfiguration();
                    break;
                case RepositoryEventType.Load:
                    break;
                case RepositoryEventType.Remove:
                    break;
                case RepositoryEventType.RemoveAll:
                    break;
                case RepositoryEventType.Fetch:
                    break;
                default:
                    throw new Exception("Unhandled Repository Event Type");
            }
        }

        private void FromConfiguration()
        {
            _controller.GetConfiguration(configuration =>
            {
                _viewModel.Directory = configuration.Directory;
                _viewModel.User = configuration.User;
                _viewModel.Password = configuration.Password;
                _viewModel.Repositories.Clear();

                //this.PasswordTB.Password = configuration.Password;

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
                    var repositoryLog = _logManager.Get(repository.Name);

                    foreach (var message in repositoryLog.Messages)
                    {
                        repositoryViewModel.Log.Add(new GitManagerLogMessageViewModel()
                        {
                            Timestamp = message.Timestamp,
                            Message = message.Message,
                        });
                    }

                    _viewModel.Repositories.Add(repositoryViewModel);
                }
            });

            // Event already sent from initialization
            this.StatusTB.Text = "Configuration Loaded:  " + _controller.GetConfigurationFile();
        }

        private async void ToConfiguration()
        {
            await _controller.SetConfiguration(configuration =>
            {
                configuration.Directory = _viewModel.Directory;
                configuration.User = _viewModel.User;
                configuration.Password = _viewModel.Password;

                foreach (var repository in _viewModel.Repositories)
                {
                    var repo = configuration.Repositories.FirstOrDefault(x => x.Name == repository.Name);

                    if (repo == null)
                    {
                        repo = new GitRepositoryStub();

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
                ToConfiguration();

                _controller.Dispose();
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

        private void AddRepositoryButton_Click(object sender, RoutedEventArgs e)
        {
            //var newRepository = new GitManagerRepositoryViewModel();

            //_dialogController.ShowDialogWindowSync(new DialogEventData(newRepository));
        }

        private async void FetchRepositoryButton_Click(object sender, RoutedEventArgs e)
        {
            //var selectedItems = this.RepoLB.SelectedItems.Cast<GitManagerRepositoryViewModel>().ToList();

            //foreach (GitManagerRepositoryViewModel repository in selectedItems)
            //{
            //    this.StatusTB.Text = "Fetching Repository (see log):  " + repository.Name;

            //    await _controller.GetRepository(repository.Name);

            //    this.StatusTB.Text = "Fetch Complete (see log):  " + repository.Name;
            //}
        }

        private void RunScriptButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PasswordTB_PasswordChanged(object sender, RoutedEventArgs e)
        {
            //_viewModel.Password = this.PasswordTB.Password;
        }
    }
}