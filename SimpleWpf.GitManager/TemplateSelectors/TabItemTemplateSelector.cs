using System.Windows;
using System.Windows.Controls;

using SimpleWpf.GitManager.ViewModel;

namespace SimpleWpf.GitManager.TemplateSelectors
{
    public class TabItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var viewModel = item as TabViewModel;
            var presenter = container as ContentPresenter;

            if (viewModel != null && presenter != null)
            {
                switch (viewModel.Type)
                {
                    case TabType.Configuration:
                        return presenter.FindResource("TabConfigurationDataTemplate") as DataTemplate;
                    case TabType.Repository:
                        return presenter.FindResource("TabRepositoryDataTemplate") as DataTemplate;
                    default:
                        throw new Exception("Unhandled template type");
                }
            }
            else
                throw new Exception("Invalid view model type or use of template selector");
        }
    }
}
