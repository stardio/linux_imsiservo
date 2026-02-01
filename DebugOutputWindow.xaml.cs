using System.Windows;

namespace EtherCAT_Studio
{
    public partial class DebugOutputWindow : Window
    {
        public DebugOutputWindow(string outputText)
        {
            InitializeComponent();
            OutputTextBox.Text = outputText;
        }

        private void SaveToFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FileName = $"debug_output_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    System.IO.File.WriteAllText(dlg.FileName, OutputTextBox.Text);
                    MessageBox.Show($"Saved to: {dlg.FileName}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
