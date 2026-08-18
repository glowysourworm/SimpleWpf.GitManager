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
        public GitManagerBootstrapper() : base(true, false)
        {

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
