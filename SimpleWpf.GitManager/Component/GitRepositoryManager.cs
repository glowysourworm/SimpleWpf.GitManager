using SimpleWpf.GitManager.Interface;
using SimpleWpf.IocFramework.Application.Attribute;

namespace SimpleWpf.GitManager.Component
{
    [IocExport(typeof(IGitRepositoryManager))]
    public class GitRepositoryManager : IGitRepositoryManager
    {
        public GitRepositoryManager()
        {

        }
    }
}
