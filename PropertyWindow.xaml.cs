using System.Windows;

namespace EtherCAT_Studio
{
    public partial class PropertyWindow : Window
    {
        public PropertyWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            // 여기에 데이터 저장 로직 추가 가능
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}