using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Controls;

namespace EtherCAT_Studio
{
    public partial class CircularMoveControl : UserControl
    {
        public CircularMoveControl()
        {
            InitializeComponent();
            DirectionBox.SelectedIndex = 0;
        }

        public void Load(JsonElement? root)
        {
            if (root == null) return;
            
            if (root.Value.TryGetProperty("center", out var center))
            {
                if (center.TryGetProperty("X", out var cx)) CenterXBox.Text = cx.GetRawText();
                if (center.TryGetProperty("Y", out var cy)) CenterYBox.Text = cy.GetRawText();
            }
            
            if (root.Value.TryGetProperty("end", out var end))
            {
                if (end.TryGetProperty("X", out var ex)) EndXBox.Text = ex.GetRawText();
                if (end.TryGetProperty("Y", out var ey)) EndYBox.Text = ey.GetRawText();
            }
            
            if (root.Value.TryGetProperty("direction", out var dir))
            {
                DirectionBox.Text = dir.GetString() ?? "CW";
            }
        }

        public Dictionary<string, object> Collect()
        {
            double centerX = 0, centerY = 0, endX = 0, endY = 0;
            double.TryParse(CenterXBox.Text, out centerX);
            double.TryParse(CenterYBox.Text, out centerY);
            double.TryParse(EndXBox.Text, out endX);
            double.TryParse(EndYBox.Text, out endY);
            
            string direction = DirectionBox.Text ?? "CW";
            
            return new Dictionary<string, object>
            {
                ["center"] = new { X = centerX, Y = centerY },
                ["end"] = new { X = endX, Y = endY },
                ["direction"] = direction
            };
        }
    }
}
