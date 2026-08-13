using System.Windows;

namespace SimpleWpf.GitManager
{
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // DialogResult:  Editor controls are automatic save
            //
            this.DialogResult = true;
        }
    }
}
