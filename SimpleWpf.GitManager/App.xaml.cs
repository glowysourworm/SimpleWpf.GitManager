using System.Windows;

namespace SimpleWpf.GitManager
{
    public partial class App : Application
    {
        GitManagerBootstrapper _bootstrapper;

        public App()
        {
            _bootstrapper = new GitManagerBootstrapper();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Next, initialize the bootstrapper
            _bootstrapper.Initialize();

            // Loads configuration prior to other injectors (MainViewModel needs Configuration)

            // Run() -> Window.Show()
            _bootstrapper.Run();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }

}
