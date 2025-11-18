using System.Windows;
using System.Windows.Input;

namespace SchneidMaschine.pages
{
    public partial class KeyInputDialog : Window
    {
        public Key SelectedKey { get; private set; }

        public KeyInputDialog()
        {
            InitializeComponent();
            SelectedKey = Key.None;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Ignoriere System-Tasten wie Alt, Ctrl, Shift alleine
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                e.Key == Key.LeftShift || e.Key == Key.RightShift ||
                e.Key == Key.LWin || e.Key == Key.RWin ||
                e.Key == Key.System)
            {
                return;
            }

            SelectedKey = e.Key;
            TextBlockKey.Text = SelectedKey.ToString();
            BtnOK.IsEnabled = true;

            e.Handled = true;
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
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
