using System.Windows;

namespace EtherCAT_Studio
{
    public partial class PropertyWindow : Window
    {
        public string? JsonResult { get; private set; }

        public PropertyWindow(string? initialJson = null)
        {
            InitializeComponent();
            // 기본 Axis 선택
            AxisBox.SelectedIndex = 0;
            if (!string.IsNullOrEmpty(initialJson))
            {
                try
                {
                    var doc = System.Text.Json.JsonDocument.Parse(initialJson);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("label", out var lbl)) LabelBox.Text = lbl.GetString() ?? string.Empty;
                    if (root.TryGetProperty("jump_target", out var jt)) JumpTargetBox.Text = jt.GetString() ?? string.Empty;
                    if (root.TryGetProperty("axis", out var a)) AxisBox.Text = a.GetString() ?? "X";
                    if (root.TryGetProperty("position", out var p)) PositionBox.Text = p.GetRawText();
                    else if (root.TryGetProperty("pos", out var p2)) PositionBox.Text = p2.GetRawText();
                    if (root.TryGetProperty("speed", out var s)) SpeedBox.Text = s.GetRawText();
                    if (root.TryGetProperty("description", out var d)) DescriptionBox.Text = d.GetString() ?? string.Empty;
                }
                catch { }
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            string label = LabelBox.Text ?? string.Empty;
            string jumpTarget = JumpTargetBox.Text ?? string.Empty;
            string axis = AxisBox.Text ?? "X";
            double pos = 0;
            double speed = 0;
            double.TryParse(PositionBox.Text, out pos);
            double.TryParse(SpeedBox.Text, out speed);
            string desc = DescriptionBox.Text ?? string.Empty;

            var obj = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(label)) obj["label"] = label;
            if (!string.IsNullOrWhiteSpace(jumpTarget)) obj["jump_target"] = jumpTarget;
            obj["axis"] = axis;
            obj["position"] = pos;
            obj["speed"] = speed;
            obj["description"] = desc;
            JsonResult = System.Text.Json.JsonSerializer.Serialize(obj, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
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