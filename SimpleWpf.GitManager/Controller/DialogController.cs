using System.Windows;

using SimpleWpf.GitManager.Event;
using SimpleWpf.GitManager.Interface;
using SimpleWpf.GitManager.View;
using SimpleWpf.IocFramework.Application.Attribute;
using SimpleWpf.IocFramework.EventAggregation;

namespace SimpleWpf.GitManager.Controller
{
    [IocExport(typeof(IDialogController))]
    public class DialogController : IDialogController
    {
        private readonly IIocEventAggregator _eventAggregator;

        private DialogWindow _dialogWindow;

        [IocImportingConstructor]
        public DialogController(IIocEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _dialogWindow = null;

            eventAggregator.GetEvent<DialogEvent>().Subscribe(payload => OnLoadingChanged(payload));
        }

        public bool ShowDialogWindowSync(DialogEventData eventData)
        {
            if (!eventData.UserDismissalMode)
                throw new Exception("Must have user dismissal mode for synchronous dialog use");

            var ready = LoadDialogWindow(eventData);

            if (ready)
            {
                var result = _dialogWindow.ShowDialog() ?? false;

                _dialogWindow.Close();
                _dialogWindow = null;

                return result;
            }
            else
                throw new Exception("Synchronous use of dialog interrupted another dialog event. Must first dismiss the other dialog window.");
        }

        private void OnLoadingChanged(DialogEventData data)
        {
            // Create / Destroy
            var ready = LoadDialogWindow(data);

            if (ready)
            {
                _dialogWindow.Show();
            }
        }

        // Returns true if the dialog is ready to show
        private bool LoadDialogWindow(DialogEventData data)
        {
            // Dismiss
            if (!data.Show)
            {
                if (_dialogWindow != null)
                {
                    _dialogWindow.Close();
                    _dialogWindow = null;
                }

                // Finished with our task.
                return false;
            }

            // Create Dialog
            else
            {
                if (_dialogWindow == null)
                {
                    _dialogWindow = new DialogWindow();
                }

                else
                    throw new Exception("Unhandled closing of current dialog. Must send dialog finished event (IsLoading = false)");
            }

            // Window.DataContext Binding:  We're using the data context to add the content presenter's data.
            //                              This is because there is no control template for the window's content.
            //                              Apparently, this is a common pattern for custom dialogs in WPF.
            //
            //                              The inner data context is for the actual data for the view. This binding
            //                              should behave as normal.
            //
            switch (data.View)
            {
                case DialogView.Log:
                    _dialogWindow.DataContext = new LogView()
                    {
                        DataContext = data.DataContext
                    };
                    break;
                case DialogView.AddRepository:
                    _dialogWindow.DataContext = new AddRepositoryView()
                    {
                        DataContext = data.DataContext
                    };
                    break;
                default:
                    throw new Exception("Unhandled dialog view type:  DialogController.cs");
            }

            // We can't add this to the binding data because it is for the window. The data context is now being used
            // for the actual view content. So, we should try to keep this pattern so this dialog controller owns the
            // DialogWindow.
            //
            _dialogWindow.TitleTB.Text = data.DialogTitle;
            _dialogWindow.HeaderContainer.Visibility = string.IsNullOrEmpty(data.DialogTitle) ? Visibility.Collapsed : Visibility.Visible;
            _dialogWindow.ButtonPanel.Visibility = data.UserDismissalMode ? Visibility.Visible : Visibility.Collapsed;
            _dialogWindow.Height = data.DialogHeight;
            _dialogWindow.Width = data.DialogWidth;

            // Can't show the loading screen as a dialog window; but the window will appear as
            // a non-closeable window.
            _dialogWindow.Owner = Application.Current.MainWindow;

            return true;
        }

        public void Dispose()
        {
            if (_dialogWindow != null)
            {
                _dialogWindow.Close();
                _dialogWindow = null;
            }
        }
    }
}
