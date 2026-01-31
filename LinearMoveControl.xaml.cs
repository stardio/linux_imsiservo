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
                // handle either numeric fields or object-with-value
                if (t.TryGetProperty("X", out var tx))
                {
                    TargetXBox.Text = tx.ValueKind == JsonValueKind.Object && tx.TryGetProperty("value", out var vtx) ? vtx.GetRawText() : tx.GetRawText();
                }
                if (t.TryGetProperty("Y", out var ty))
                {
                    TargetYBox.Text = ty.ValueKind == JsonValueKind.Object && ty.TryGetProperty("value", out var vty) ? vty.GetRawText() : ty.GetRawText();
                }
                if (t.TryGetProperty("Z", out var tz))
                {
                    TargetZBox.Text = tz.ValueKind == JsonValueKind.Object && tz.TryGetProperty("value", out var vtz) ? vtz.GetRawText() : tz.GetRawText();
                }
            }
            if (root.Value.TryGetProperty("speed", out var s))
            {
                LinearSpeedBox.Text = s.ValueKind == JsonValueKind.Object && s.TryGetProperty("value", out var vs) ? vs.GetRawText() : s.GetRawText();
            }
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
                ["target"] = new Dictionary<string, object> { ["X"] = new { value = tx, unit = "Puls" }, ["Y"] = new { value = ty, unit = "Puls" }, ["Z"] = new { value = tz, unit = "Puls" } },
                ["speed"] = new { value = speed, unit = "Puls" }
            };
        }
    }
}