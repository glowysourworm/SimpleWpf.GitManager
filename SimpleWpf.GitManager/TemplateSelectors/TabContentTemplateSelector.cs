using System.Windows;
using System.Windows.Controls;

using SimpleWpf.GitManager.ViewModel;

namespace SimpleWpf.GitManager.TemplateSelectors
{
    public class TabContentTemplateSelector : DataTemplateSelector
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
                        return presenter.FindResource("TabContentTemplate") as DataTemplate;
                    case TabType.Repository:
                        return presenter.FindResource("TabCloseableContentTemplate") as DataTemplate;
                    default:
                        throw new Exception("Unhandled template type");
                }
            }
            else
                throw new Exception("Invalid view model type or use of template selector");
        }
    }
}
