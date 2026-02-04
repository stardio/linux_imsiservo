using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace EtherCAT_Studio
{
    public partial class AIPromptWindow : UserControl
    {
        public string? GeneratedJson { get; private set; }
        public bool IsConfirmed { get; private set; }
        public string OllamaBaseUrl { get; set; } = "http://localhost:11434";

        private SequenceJsonGenerator? _generator;

        public AIPromptWindow()
        {
            InitializeComponent();
        }

        private async void GenerateBtn_Click(object sender, RoutedEventArgs e)
        {
            string prompt = PromptInput.Text;
            if (string.IsNullOrWhiteSpace(prompt))
            {
                MessageBox.Show("프롬프트를 입력해주세요");
                return;
            }

            GenerateBtn.IsEnabled = false;
            GenerateBtn.Content = "생성 중...";

            try
            {
                _generator = new SequenceJsonGenerator(OllamaBaseUrl);
                GeneratedJson = await _generator.GenerateSequenceJsonAsync(prompt);
                IsConfirmed = true;

                MessageBox.Show("시퀀스 생성 완료!");
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"오류: {ex.Message}\n\n{ex.StackTrace}");
                GenerateBtn.IsEnabled = true;
                GenerateBtn.Content = "생성";
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            Window.GetWindow(this)?.Close();
        }
    }
}
