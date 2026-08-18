using System.Windows;

using SimpleWpf.GitManager.Event;
using SimpleWpf.GitManager.Interface;
using SimpleWpf.GitManager.ViewModel;
using SimpleWpf.IocFramework.Application;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.IocFramework.RegionManagement.Interface;

namespace SimpleWpf.GitManager
{
    [IocExportDefault]
    public class MainModule : ModuleBase
    {
        static string DEFAULT_CONFIGURATION = "GitManager.json";

        private readonly IIocEventAggregator _eventAggregator;
        private readonly IGitController _controller;

        [IocImportingConstructor]
        public MainModule(IIocRegionManager regionManager, IIocEventAggregator eventAggregator, IGitController controller) : base(regionManager, eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _controller = controller;
        }

        public override async void Run()
        {
            // Show Main Window
            base.Run();

            var dialogViewModel = new GitManagerLoadingViewModel()
            {
                Loading = true,
                ProgressMessage = "Opening Configuration",
                ProgressPercent = 0,
                ShowProgress = false
            };

            _eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData("Initializing", dialogViewModel));

            try
            {
                // Get config file from the command line (or default to config folder as current executable directory)
                var configurationFile = Environment.GetCommandLineArgs().Length > 1 ? Environment.GetCommandLineArgs()[1] : DEFAULT_CONFIGURATION;

                await _controller.OpenConfiguration(configurationFile);
            }
            catch (Exception ex)
            {
                // Dismiss
                _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());

                MessageBox.Show("Error loading configuration", ex.Message);

                // Skip Initialization
                return;
            }

            // Dismiss
            _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());

            // Dialog: Reload for progress
            dialogViewModel.ProgressMessage = "Opening Repositories...";

            _eventAggregator.GetEvent<DialogEvent>().Publish(new DialogEventData("Initializing", dialogViewModel));

            try
            {
                await _controller.Initialize();
            }
            catch (Exception ex)
            {
                // Dismiss
                _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());

                MessageBox.Show("Error opening repositories", ex.Message);
                return;
            }

            // Dismiss (success!)
            _eventAggregator.GetEvent<DialogEvent>().Publish(DialogEventData.Dismiss());
        }
    }
}
