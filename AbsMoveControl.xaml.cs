using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Controls;

namespace EtherCAT_Studio
{
    public partial class AbsMoveControl : UserControl
    {
        public AbsMoveControl()
        {
            InitializeComponent();
        }

        public void Load(JsonElement? root)
        {
            if (root == null) return;
            if (root.Value.TryGetProperty("axis", out var a)) AxisBox.Text = a.GetString() ?? "X";
            else AxisBox.SelectedIndex = 0;
            // support target_position (new) or position/pos (legacy)
            if (root.Value.TryGetProperty("target_position", out var tp))
            {
                PositionBox.Text = tp.ValueKind == JsonValueKind.Object && tp.TryGetProperty("value", out var vtp) ? vtp.GetRawText() : tp.GetRawText();
            }
            else if (root.Value.TryGetProperty("position", out var p))
            {
                PositionBox.Text = p.ValueKind == JsonValueKind.Object && p.TryGetProperty("value", out var vp) ? vp.GetRawText() : p.GetRawText();
            }
            else if (root.Value.TryGetProperty("pos", out var p2)) PositionBox.Text = p2.GetRawText();

            if (root.Value.TryGetProperty("speed", out var s)) SpeedBox.Text = s.ValueKind == JsonValueKind.Object && s.TryGetProperty("value", out var vs) ? vs.GetRawText() : s.GetRawText();
        }

        public Dictionary<string, object> Collect()
        {
            string axis = AxisBox.Text ?? "X";
            double pos = 0, speed = 0;
            double.TryParse(PositionBox.Text, out pos);
            double.TryParse(SpeedBox.Text, out speed);
            var dict = new Dictionary<string, object>
            {
                ["axis"] = axis,
                ["target_position"] = new { value = pos, unit = "Puls" },
                ["speed"] = new { value = speed, unit = "Puls" }
            };
            return dict;
        }
    }
}
