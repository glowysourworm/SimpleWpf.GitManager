using System.Windows;
using System.Windows.Controls;

using SimpleWpf.GitManager.ViewModel;

namespace SimpleWpf.GitManager.StyleSelectors
{
    public class TabStyleSelector : StyleSelector
    {
        public override Style SelectStyle(object item, DependencyObject container)
        {
            var presenter = container as TabItem;
            var viewModel = item as TabViewModel;

            if (viewModel == null || presenter == null)
                throw new NullReferenceException("Improper handling of TabStyleSelector");

            if (viewModel != null && presenter != null)
            {
                switch (viewModel.Type)
                {
                    case TabType.Configuration:
                        return presenter.FindResource("TabItemClosableStyle") as Style;
                    case TabType.Repository:
                        return presenter.FindResource("TabItemStyle") as Style;
                    default:
                        throw new Exception("Unhandled style type");
                }
            }
            else
                throw new Exception("Invalid view model type or use of style selector");
        }
    }
}
