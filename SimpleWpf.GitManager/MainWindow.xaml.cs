using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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

            _viewModel = new GitManagerViewModel();
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
                    Message = eventData.Data.Message.Trim(),
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
                    await _controller.SetConfiguration(configuration =>
                    {
                        configuration.Directory = _viewModel.Directory;

                    }, true);
                }
                break;
                case ViewEventType.RepositoryViewRequest:
                {
                    var repository = _viewModel.Repositories.First(x => x.Name == eventData.RepositoryName);

                    // Existing Tab
                    if (_viewModel.Tabs.Any(x => x.Header == repository.Name))
                    {
                        var tabItem = this.MainTabCtrl.Items.First<TabItem>(x => (x as TabItem).DataContext == repository);

                        this.MainTabCtrl.SelectedItem = tabItem;
                    }

                    // New Tab
                    else
                    {
                        _viewModel.Tabs.Add(new TabViewModel()
                        {
                            Header = repository.Name,
                            Type = TabType.Repository,
                            TabDataContext = repository
                        });
                    }
                }
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
                case RepositoryEventType.Load:
                    break;
                case RepositoryEventType.Remove:
                    Repository_Remove(eventData.RepositoryName);
                    break;
                case RepositoryEventType.RemoveAll:
                    Repository_RemoveAll();
                    break;
                case RepositoryEventType.Fetch:
                    break;
                default:
                    throw new Exception("Unhandled event data type");
            }
        }

        private void Repository_Add(string repositoryName)
        {
            var viewModel = _viewModel.Repositories.FirstOrDefault(x => x.Name == repositoryName);

            // Add
            if (viewModel == null)
            {
                viewModel = new GitManagerRepositoryViewModel();

                _viewModel.Repositories.Add(viewModel);
            }

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
            _viewModel.Repositories.Remove(x => x.Name == repositoryName);
            _viewModel.Tabs.Remove(x => x.Header == repositoryName);
        }

        private void Repository_RemoveAll()
        {
            _viewModel.Repositories.Clear();
            _viewModel.Tabs.Remove(x => x.IsClosable);
        }

        private void FromConfiguration()
        {
            _controller.GetConfiguration(configuration =>
            {
                _viewModel.Directory = configuration.Directory;
                _viewModel.User = configuration.User;
                _viewModel.Password = configuration.Password;
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

            }, true);
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

        private void MapRepository(GitRepositoryStub source, ref GitManagerRepositoryViewModel destination)
        {
            destination.LastCommitLocal = source.LastCommitLocal;
            destination.LastCommitRemote = source.LastCommitRemote;
            destination.LastFetch = source.LastFetch;
            destination.IsFork = source.IsFork;
            destination.BaseDirectory = source.BaseDirectory;
            destination.Name = source.Name;
            destination.Size = source.Size;
            destination.GitUrl = source.GitUrl;
        }

        private void AddRepositoryButton_Click(object sender, RoutedEventArgs e)
        {
            //var newRepository = new GitManagerRepositoryViewModel();

            //_dialogController.ShowDialogWindowSync(new DialogEventData(newRepository));
        }

        private async void FetchRepositoryButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = _viewModel.Repositories.Where(x => x.IsSelected);

            foreach (GitManagerRepositoryViewModel repository in selectedItems)
            {
                this.StatusTB.Text = "Fetching Repository (see log):  " + repository.Name;

                repository.IsLoading = true;
                repository.LoadingMessage = "Performing Fetch...";

                await _controller.Fetch(repository.Name);

                repository.IsLoading = false;
                repository.LoadingMessage = string.Empty;

                this.StatusTB.Text = "Fetch Complete (see log):  " + repository.Name;
            }
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