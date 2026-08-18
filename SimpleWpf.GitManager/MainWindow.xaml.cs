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

        readonly string SHUTDOWN_ERROR_MSG = "Error shutting down Git Manager. Shutdown anyway? Your repository data in the configuration may be lost!";

        GitManagerViewModel _viewModel;

        // Designer Constructor
        public MainWindow()
        {
            InitializeComponent();
        }

        [IocImportingConstructor]
        public MainWindow(IIocEventAggregator eventAggregator,
                          IGitController controller,
                          IDialogController dialogController)
        {
            _eventAggregator = eventAggregator;
            _controller = controller;
            _dialogController = dialogController;
            _viewModel = new GitManagerViewModel();

            // Tabs
            _viewModel.Tabs.Add(new TabViewModel()
            {
                Header = "Configuration",
                TabDataContext = _viewModel,
                Type = TabType.Configuration,
                IsSelected = true,
                IsClosable = true
            });

            InitializeComponent();

            // Configuration (Loaded)
            eventAggregator.GetEvent<ConfigurationEvent>().Subscribe(configuration =>
            {
                BasicHelpers.InvokeDispatcher(OnConfigurationChanged, DispatcherPriority.Background, configuration);
            });

            eventAggregator.GetEvent<LogEvent>().Subscribe(log =>
            {
                BasicHelpers.InvokeDispatcher(OnLog, DispatcherPriority.Background, log);
            });

            eventAggregator.GetEvent<ViewEvent>().Subscribe(data =>
            {
                BasicHelpers.InvokeDispatcher(OnViewEvent, DispatcherPriority.Background, data);
            });

            eventAggregator.GetEvent<RepositoryViewModelEvent>().Subscribe(data =>
            {
                BasicHelpers.InvokeDispatcher(OnRepositoryEvent, DispatcherPriority.Background, data);
            });

            eventAggregator.GetEvent<DialogEvent>().Subscribe(data =>
            {
                BasicHelpers.InvokeDispatcher(OnDialogEvent, DispatcherPriority.Background, data);
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
            if (_viewModel.HasRepository(eventData.RepositoryName))
            {
                var repository = _viewModel.GetRepository(eventData.RepositoryName);

                repository.Log.Add(new GitManagerLogMessageViewModel()
                {
                    Timestamp = eventData.Data.Timestamp,
                    Message = eventData.Data.Message.Trim(),
                });
            }
        }

        private async void OnViewEvent(ViewEventData eventData)
        {
            switch (eventData.Type)
            {
                case ViewEventType.ConfigurationModified:
                    ToConfiguration(false);
                    break;
                case ViewEventType.ConfigurationModifiedReload:
                    ToConfiguration(true);
                    break;
                case ViewEventType.RepositoryViewRequest:
                    Repository_View(eventData.RepositoryName);
                    break;
                default:
                    throw new Exception("Unhandled view event type");
            }
        }

        private void OnRepositoryEvent(RepositoryEventData eventData)
        {
            // Procedure:  The data has been loaded from the back end. Use the IGitController
            //             to get repository information and log.

            switch (eventData.EventType)
            {
                case RepositoryEventType.Add:
                    Repository_Add(eventData.RepositoryName);
                    break;
                case RepositoryEventType.Update:
                    //Repository_View(eventData.RepositoryName);
                    break;
                case RepositoryEventType.Remove:
                    Repository_Remove(eventData.RepositoryName);
                    break;
                case RepositoryEventType.RemoveAll:
                    Repository_RemoveAll();
                    break;
                case RepositoryEventType.Fetch:
                    Repository_View(eventData.RepositoryName);
                    break;
                default:
                    throw new Exception("Unhandled event data type");
            }
        }

        private void OnDialogEvent(DialogEventData data)
        {
            var viewModel = data.DataContext as GitManagerLoadingViewModel;

            _viewModel.Loading = data.DataContext == null ? false : viewModel.Loading;
        }

        private void Repository_Add(string repositoryName)
        {
            var hasRepository = _viewModel.HasRepository(repositoryName);

            GitManagerRepositoryViewModel viewModel = null;

            // Add
            if (!hasRepository)
            {
                viewModel = new GitManagerRepositoryViewModel();
                _viewModel.AddRepository(viewModel);
            }
            else
                viewModel = _viewModel.GetRepository(repositoryName);

            // Update -> (property listener in the view model)
            MapRepository(_controller.GetRepository(repositoryName), ref viewModel);

            // Log
            viewModel.Log.Clear();

            var logMessages = _controller.GetRepositoryLog(repositoryName);

            foreach (var logMessage in logMessages.Messages)
            {
                viewModel.Log.Add(new GitManagerLogMessageViewModel()
                {
                    Message = logMessage.Message,
                    Timestamp = logMessage.Timestamp
                });
            }
        }

        private void Repository_Remove(string repositoryName)
        {
            _viewModel.RemoveRepository(repositoryName);
            _viewModel.Tabs.Remove(x => x.Header == repositoryName);
        }

        private void Repository_RemoveAll()
        {
            _viewModel.ClearAllRepositories();
            _viewModel.Tabs.Remove(x => x.IsClosable);
        }

        private void Repository_View(string repositoryName)
        {
            var repository = _viewModel.GetRepository(repositoryName);

            // Existing Tab
            if (_viewModel.Tabs.Any(x => x.Header == repository.Name))
            {
                var tabItem = this.MainTabCtrl.Items.First<TabViewModel>(x => x.TabDataContext == repository);

                this.MainTabCtrl.SelectedItem = tabItem;
            }

            // New Tab
            else
            {
                var tabItem = new TabViewModel()
                {
                    Header = repository.Name,
                    Type = TabType.Repository,
                    TabDataContext = repository,
                    IsSelected = true,
                    IsClosable = true
                };

                _viewModel.Tabs.Add(tabItem);
                this.MainTabCtrl.SelectedItem = tabItem;
            }
        }

        private void FromConfiguration()
        {
            var configuration = _controller.GetConfiguration();

            _viewModel.Directory = configuration.Directory;
            _viewModel.User = configuration.User;
            _viewModel.Password = configuration.Password;

            // Event already sent from initialization
            this.StatusTB.Text = "Configuration Loaded:  " + _controller.GetConfigurationFile();
        }

        private async void ToConfiguration(bool reload)
        {
            await _controller.SetConfiguration(configuration =>
            {
                configuration.Directory = _viewModel.Directory;
                configuration.User = _viewModel.User;
                configuration.Password = _viewModel.Password;

            }, reload);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // TODO:  Create Bootstrapper Logic
            try
            {
                _controller.SaveConfiguration();

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

        private void MapRepository(GitRepositoryStub source, ref GitManagerRepositoryViewModel destination)
        {
            destination.AheadBy = source.AheadBy;
            destination.BaseDirectory = source.WorkingDirectory;
            destination.BehindBy = source.BehindBy;
            destination.CommitLocalMessage = source.LastCommitLocal?.Message ?? string.Empty;
            destination.CommitLocalUser = source.LastCommitLocal?.Author ?? string.Empty;
            destination.CommitLocalWhen = source.LastCommitLocal?.Timestamp ?? DateTime.MinValue;
            destination.CommitRemoteMessage = source.LastCommitRemote?.Message ?? string.Empty;
            destination.CommitRemoteUser = source.LastCommitRemote?.Author ?? string.Empty;
            destination.CommitRemoteWhen = source.LastCommitRemote?.Timestamp ?? DateTime.MinValue;
            destination.IsAhead = source.IsAhead;
            destination.IsBehind = source.IsBehind;
            destination.IsFork = source.IsFork;
            destination.IsLoading = false;
            destination.IsSelected = false;
            destination.LoadingMessage = string.Empty;
            destination.Name = source.Name;
            destination.OwnerName = source.OwnerName;
            destination.RemoteHead = string.Empty;
            destination.RemoteSize = source.RemoteSize;
            destination.Url = source.Url;
            destination.WorkingDirectory = source.WorkingDirectory;
        }

        private void AddRepositoryButton_Click(object sender, RoutedEventArgs e)
        {
            //var newRepository = new GitManagerRepositoryViewModel();

            //_dialogController.ShowDialogWindowSync(new DialogEventData(newRepository));
        }

        private async void FetchRepositoryButton_Click(object sender, RoutedEventArgs e)
        {
            // Fetch only repositories that are behind
            var selectedItems = _viewModel.RepositoriesBehind.Where(x => x.IsSelected);

            foreach (GitManagerRepositoryViewModel repository in selectedItems)
            {
                this.StatusTB.Text = "Fetching Repository (see log):  " + repository.Name;

                repository.IsLoading = true;
                repository.LoadingMessage = "Performing Fetch...";

                var repositoryStub = _controller.GetRepository(repository.Name);

                await _controller.Fetch(repositoryStub);

                repository.IsLoading = false;
                repository.LoadingMessage = string.Empty;

                this.StatusTB.Text = "Fetch Complete (see log):  " + repository.Name;
            }
        }
    }
}