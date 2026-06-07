using System.Windows;

namespace RenPyTRLauncher.Views
{
    public partial class InputDialog : Window
    {
        public string Result { get; private set; } = string.Empty;

        public InputDialog(string prompt, string defaultValue = "")
        {
            InitializeComponent();
            TxtPrompt.Text = prompt;
            TxtInput.Text = defaultValue;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Result = TxtInput.Text;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
