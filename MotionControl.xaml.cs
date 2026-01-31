using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Controls;

namespace EtherCAT_Studio
{
    public partial class MotionControl : UserControl
    {
        private bool _isAbsMove = false;
        public bool IsAbsMove
        {
            get => _isAbsMove;
            set
            {
                _isAbsMove = value;
                // hide/show UI elements
                if (SpeedHistoryBox != null) SpeedHistoryBox.Visibility = _isAbsMove ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
                if (DescriptionBox != null) DescriptionBox.Visibility = _isAbsMove ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            }
        }

        public MotionControl()
        {
            InitializeComponent();
        }

        public void Load(JsonElement? root)
        {
            // apply mode before populating
            if (IsAbsMove)
            {
                SpeedHistoryBox.Visibility = System.Windows.Visibility.Collapsed;
                DescriptionBox.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (root == null) return;
            if (root.Value.TryGetProperty("axis", out var a)) AxisBox.Text = a.GetString() ?? "X";
            else AxisBox.SelectedIndex = 0;
            // support new key "target_position", fallback to old "position" and "pos"
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
            if (root.Value.TryGetProperty("description", out var d)) DescriptionBox.Text = d.GetString() ?? string.Empty;
            // optional history: if a "speed_history" array present, populate
            if (!IsAbsMove && root.Value.TryGetProperty("speed_history", out var sh) && sh.ValueKind == JsonValueKind.Array)
            {
                SpeedHistoryBox.Items.Clear();
                foreach (var item in sh.EnumerateArray()) SpeedHistoryBox.Items.Add(item.GetRawText());
            }
        }

        public Dictionary<string, object> Collect()
        {
            string axis = AxisBox.Text ?? "X";
            double pos = 0, speed = 0;
            double.TryParse(PositionBox.Text, out pos);
            double.TryParse(SpeedBox.Text, out speed);
            string desc = DescriptionBox.Text ?? string.Empty;
            var dict = new Dictionary<string, object>
            {
                ["axis"] = axis,
                ["target_position"] = new { value = pos, unit = "Puls" },
                ["speed"] = new { value = speed, unit = "Puls" }
            };
            // include description only if not ABS MOVE
            if (!IsAbsMove)
            {
                dict["description"] = desc;
                // include speed history if any
                if (SpeedHistoryBox.Items.Count > 0)
                {
                    var list = new List<double>();
                    foreach (var it in SpeedHistoryBox.Items)
                    {
                        if (double.TryParse(it?.ToString(), out var v)) list.Add(v);
                    }
                    if (list.Count > 0) dict["speed_history"] = list;
                }
            }
            return dict;
        }
    }
}