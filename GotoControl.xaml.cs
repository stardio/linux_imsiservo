using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Controls;

namespace EtherCAT_Studio
{
    public partial class GotoControl : UserControl
    {
        public GotoControl()
        {
            InitializeComponent();
        }

        public void Load(JsonElement? root)
        {
            if (root == null) return;
            if (root.Value.TryGetProperty("to", out var t)) ToBox.Text = t.GetString() ?? string.Empty;
            if (root.Value.TryGetProperty("description", out var d)) DescriptionBox.Text = d.GetString() ?? string.Empty;
        }

        public Dictionary<string, object> Collect()
        {
            string to = ToBox.Text ?? string.Empty;
            string desc = DescriptionBox.Text ?? string.Empty;
            var dict = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(to)) dict["to"] = to;
            if (!string.IsNullOrWhiteSpace(desc)) dict["description"] = desc;
            return dict;
        }
    }
}
