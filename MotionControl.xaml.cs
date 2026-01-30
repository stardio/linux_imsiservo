using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Controls;

namespace EtherCAT_Studio
{
    public partial class MotionControl : UserControl
    {
        public MotionControl()
        {
            InitializeComponent();
        }

        public void Load(JsonElement? root)
        {
            if (root == null) return;
            if (root.Value.TryGetProperty("axis", out var a)) AxisBox.Text = a.GetString() ?? "X";
            else AxisBox.SelectedIndex = 0;
            if (root.Value.TryGetProperty("position", out var p)) PositionBox.Text = p.GetRawText();
            else if (root.Value.TryGetProperty("pos", out var p2)) PositionBox.Text = p2.GetRawText();
            if (root.Value.TryGetProperty("speed", out var s)) SpeedBox.Text = s.GetRawText();
            if (root.Value.TryGetProperty("description", out var d)) DescriptionBox.Text = d.GetString() ?? string.Empty;
        }

        public Dictionary<string, object> Collect()
        {
            string axis = AxisBox.Text ?? "X";
            double pos = 0, speed = 0;
            double.TryParse(PositionBox.Text, out pos);
            double.TryParse(SpeedBox.Text, out speed);
            string desc = DescriptionBox.Text ?? string.Empty;
            return new Dictionary<string, object>
            {
                ["axis"] = axis,
                ["position"] = pos,
                ["speed"] = speed,
                ["description"] = desc
            };
        }
    }
}