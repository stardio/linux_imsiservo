using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Controls;

namespace EtherCAT_Studio
{
    public partial class LinearMoveControl : UserControl
    {
        public LinearMoveControl()
        {
            InitializeComponent();
        }

        public void Load(JsonElement? root)
        {
            if (root == null) return;
            if (root.Value.TryGetProperty("target", out var t) && t.ValueKind == JsonValueKind.Object)
            {
                if (t.TryGetProperty("X", out var tx)) TargetXBox.Text = tx.GetRawText();
                if (t.TryGetProperty("Y", out var ty)) TargetYBox.Text = ty.GetRawText();
                if (t.TryGetProperty("Z", out var tz)) TargetZBox.Text = tz.GetRawText();
            }
            if (root.Value.TryGetProperty("speed", out var s)) LinearSpeedBox.Text = s.GetRawText();
        }

        public Dictionary<string, object> Collect()
        {
            double tx = 0, ty = 0, tz = 0, speed = 0;
            double.TryParse(TargetXBox.Text, out tx);
            double.TryParse(TargetYBox.Text, out ty);
            double.TryParse(TargetZBox.Text, out tz);
            double.TryParse(LinearSpeedBox.Text, out speed);
            return new Dictionary<string, object>
            {
                ["target"] = new { X = tx, Y = ty, Z = tz },
                ["speed"] = speed
            };
        }
    }
}