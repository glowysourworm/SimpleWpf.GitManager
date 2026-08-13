using System.Windows;

using SimpleWpf.GitManager.ViewModel;
using SimpleWpf.IocFramework.EventAggregation;
using SimpleWpf.ViewModel;

namespace SimpleWpf.GitManager.Event
{
    public enum DialogView
    {
        // Dismiss (overlaps other dismiss function)
        None,
        Log,
        AddRepository
    }

    public class DialogEventData
    {
        private static readonly int DialogDefaultHeight = 400;
        private static readonly int DialogDefaultWidth = 600;

        /// <summary>
        /// Signals that the dialog is to be displayed to the user. This does NOT imply that the
        /// dialog window will be dismissed by the user. For this, the message box buttons must
        /// be set; and only for certain dialog views.
        /// </summary>
        public bool Show { get; set; }

        /// <summary>
        /// User Dismissal Mode:  Set in the event that the dialog window is used as a send / response
        ///                       modal-interactive popup.
        /// </summary>
        public bool UserDismissalMode { get; private set; }

        /// <summary>
        /// Title for the dialog window. This will be shown centered on the banner; or the banner will be
        /// hidden.
        /// </summary>
        public string DialogTitle { get; set; }

        /// <summary>
        /// Height of the dialog window
        /// </summary>
        public int DialogHeight { get; set; }

        /// <summary>
        /// Width of the dialog window
        /// </summary>
        public int DialogWidth { get; set; }

        /// <summary>
        /// View for the content of the dialog window
        /// </summary>
        public DialogView View { get; set; }

        /// <summary>
        /// User Dismissal Mode:  Show message box buttons to the user and wait for a dialog response. This 
        ///                       should operate like a modal window. The dialog result will be completed for
        ///                       this case; and a flag will be set to show that this mode was intended.
        /// </summary>
        public MessageBoxButton MessageBoxButtons { get; set; }

        /// <summary>
        /// Data context for the dialog event:  (see AudioStation.Event.EventViewModel)
        /// </summary>
        public ViewModelBase DataContext { get; set; }

        // Could use a better way to set this (globally)
        static DialogEventData()
        {
            DialogDefaultHeight = (int)(2 * SystemParameters.MaximizedPrimaryScreenHeight / 3);
            DialogDefaultWidth = (int)(2 * SystemParameters.MaximizedPrimaryScreenWidth / 3);
        }

        public DialogEventData(bool showDialog = false)
            : this(showDialog, false, string.Empty, DialogDefaultWidth, DialogDefaultHeight, MessageBoxButton.OK, DialogView.Log, null)
        { }

        public DialogEventData(string dialogTitle, GitManagerLogViewModel viewModel)
            : this(true, true, dialogTitle, DialogDefaultWidth, DialogDefaultHeight, MessageBoxButton.OK, DialogView.Log, viewModel)
        { }

        public DialogEventData(GitManagerRepositoryViewModel viewModel)
            : this(true, true, string.Empty, DialogDefaultWidth, DialogDefaultHeight, MessageBoxButton.OK, DialogView.AddRepository, viewModel)
        { }

        private DialogEventData(bool showDialog,
                                bool userDismissal,
                                string dialogTitle,
                                int dialogWidth,
                                int dialogHeight,
                                MessageBoxButton userDismissalButtons,
                                DialogView eventView,
                                ViewModelBase viewDataContext)
        {
            this.UserDismissalMode = userDismissal;

            if (userDismissal)
            {
                // Make sure we're supporting the dismissal mode
                //
                switch (userDismissalButtons)
                {
                    case MessageBoxButton.OK:
                        this.MessageBoxButtons = userDismissalButtons;
                        break;
                    case MessageBoxButton.OKCancel:
                        this.MessageBoxButtons = userDismissalButtons;
                        break;
                    case MessageBoxButton.YesNoCancel:
                    case MessageBoxButton.YesNo:
                    default:
                        throw new Exception("Unhandled MessageBoxButton:  DialogEvent.cs");
                }
            }

            this.Show = showDialog;
            this.View = eventView;
            this.DataContext = viewDataContext;
            this.DialogTitle = dialogTitle;
            this.DialogWidth = dialogWidth;
            this.DialogHeight = dialogHeight;
        }

        /// <summary>
        /// Creates event data for dialog dismissal
        /// </summary>
        public static DialogEventData Dismiss()
        {
            return new DialogEventData(false, false, string.Empty, DialogDefaultWidth, DialogDefaultHeight, MessageBoxButton.OK, DialogView.None, null);
        }

        //public static DialogEventData ShowLoading(string title)
        //{
        //    return new DialogEventData(new DialogLoadingViewModel()
        //    {
        //        Title = title,
        //        ShowProgressBar = false,
        //        Message = string.Empty,
        //        Progress = 0
        //    });
        //}
    }

    /// <summary>
    /// Primary loading event corresponding to the MainViewModel.Loading indicator boolean. This will
    /// be utilized to hide / show loading UI and cursor.
    /// </summary>
    public class DialogEvent : IocEvent<DialogEventData>
    {
    }
}
