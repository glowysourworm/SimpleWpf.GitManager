using System.Windows;

using SimpleWpf.GitManager.Interface;
using SimpleWpf.IocFramework.Application;

namespace SimpleWpf.GitManager
{
    /// <summary>
    /// IOC Bootstrapper:  Takes over primary control / startup of the application. The configuration is
    ///                    read here; and the components are initialized. Most / all major components will
    ///                    inherit from an interface; and have Initialize / Dispose methods. These are 
    ///                    handled during the UserPreModuleInitialize sequence - after the configuration is
    ///                    read. This configuration will also be injected into the primary view model. Changes
    ///                    to the primary view model / configuration may be handled there; and disposing of
    ///                    the main components will also be handled by our IDisposable pattern.
    /// </summary>
    class GitManagerBootstrapper : IocWindowBootstrapper
    {
        static string DEFAULT_CONFIGURATION = "GitManager.json";

        public GitManagerBootstrapper() : base(false)
        {

        }

        protected override async void UserPreModuleInitialize()
        {
            // Window Management:  The shell window must be defined as the main window before
            //                     opening another window (here, the dialog). So, perhaps it 
            //                     would be best to introduce a window management system to the
            //                     IOC framework. 
            //
            // This will only call initialize on the module(s). Any other pieces will wait
            // on their injector until they're called from the container. So, the main view
            // model will wait (for the configuration) until it's used by the MainWindow.
            //
            base.UserPreModuleInitialize();

            // Initialize configuration before proceeding
            //
            // We can inject our initialize procedure(s) here
            //
            var controller = IocContainer.Get<IGitController>();

            // Get config file from the command line (or default to config folder as current executable directory)
            var configurationFile = Environment.GetCommandLineArgs().Length > 1 ? Environment.GetCommandLineArgs()[1] : DEFAULT_CONFIGURATION;

            // Read / Create Configuration
            try
            {
                await controller.Initialize(configurationFile, DEFAULT_CONFIGURATION);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Configuration Error (default GitManager.json)", ex.Message);
            }


        }

        public override IEnumerable<ModuleDefinition> DefineModules()
        {
            return new ModuleDefinition[]
            {
                new ModuleDefinition("MainModule", typeof(MainModule), true)
            };
        }

        public override Type DefineShell()
        {
            return typeof(MainWindow);
        }
    }
}
