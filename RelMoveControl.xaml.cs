using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Controls;

namespace EtherCAT_Studio
{
    public partial class RelMoveControl : UserControl
    {
        public RelMoveControl()
        {
            InitializeComponent();
            AxisBox.SelectedIndex = 0;
        }

        public void Load(JsonElement? root)
        {
            if (root == null) return;
            if (root.Value.TryGetProperty("axis", out var a)) AxisBox.Text = a.GetString() ?? "X";
            
            if (root.Value.TryGetProperty("distance", out var d))
            {
                DistanceBox.Text = d.ValueKind == JsonValueKind.Object && d.TryGetProperty("value", out var vd) 
                    ? vd.GetRawText() : d.GetRawText();
            }
            
            if (root.Value.TryGetProperty("speed", out var s))
            {
                SpeedBox.Text = s.ValueKind == JsonValueKind.Object && s.TryGetProperty("value", out var vs) 
                    ? vs.GetRawText() : s.GetRawText();
            }
        }

        public Dictionary<string, object> Collect()
        {
            string axis = AxisBox.Text ?? "X";
            double distance = 0, speed = 0;
            double.TryParse(DistanceBox.Text, out distance);
            double.TryParse(SpeedBox.Text, out speed);
            
            return new Dictionary<string, object>
            {
                ["axis"] = axis,
                ["distance"] = new { value = distance, unit = "Puls" },
                ["speed"] = new { value = speed, unit = "Puls" }
            };
        }
    }
}
