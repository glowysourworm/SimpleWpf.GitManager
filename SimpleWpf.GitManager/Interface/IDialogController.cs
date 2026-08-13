using SimpleWpf.GitManager.Event;

namespace SimpleWpf.GitManager.Interface
{
    public interface IDialogController : IDisposable
    {
        /// <summary>
        /// Shows dialog window synchronously. This represents a parallel usage to the event aggregator! So,
        /// use this when a dialog window is needed to be waited on; and the results returned immediately.
        /// </summary>
        bool ShowDialogWindowSync(DialogEventData eventData);
    }
}
