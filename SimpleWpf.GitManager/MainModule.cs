using SimpleWpf.IocFramework.Application;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.IocFramework.RegionManagement.Interface;

namespace SimpleWpf.GitManager
{
    [IocExportDefault]
    public class MainModule : ModuleBase
    {
        [IocImportingConstructor]
        public MainModule(IIocRegionManager regionManager, IIocEventAggregator eventAggregator) : base(regionManager, eventAggregator)
        {
        }

        public override void Run()
        {
            base.Run();
        }
    }
}
