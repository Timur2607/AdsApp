using System.Windows;

namespace AdsApp
{
    public partial class ProfitInputWindow : Window
    {
        public string ProfitText => ProfitTextBox.Text.Trim();

        public ProfitInputWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
