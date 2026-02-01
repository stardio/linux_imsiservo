using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Controls;

namespace EtherCAT_Studio
{
    public partial class CounterControl : UserControl
    {
        public CounterControl()
        {
            InitializeComponent();
        }

        public void Load(JsonElement? root)
        {
            if (root == null) return;
            
            if (root.Value.TryGetProperty("name", out var n)) NameBox.Text = n.GetString() ?? string.Empty;
            if (root.Value.TryGetProperty("initial", out var i)) InitialBox.Text = i.GetRawText();
            if (root.Value.TryGetProperty("target", out var t)) TargetBox.Text = t.GetRawText();
            if (root.Value.TryGetProperty("increment", out var inc)) IncrementBox.Text = inc.GetRawText();
            if (root.Value.TryGetProperty("gotoNode", out var g)) GotoNodeBox.Text = g.GetString() ?? string.Empty;
        }

        public Dictionary<string, object> Collect()
        {
            string name = NameBox.Text ?? string.Empty;
            int initial = 0, target = 0, increment = 1;
            int.TryParse(InitialBox.Text, out initial);
            int.TryParse(TargetBox.Text, out target);
            int.TryParse(IncrementBox.Text, out increment);
            string gotoNode = GotoNodeBox.Text ?? string.Empty;
            
            return new Dictionary<string, object>
            {
                ["name"] = name,
                ["initial"] = initial,
                ["target"] = target,
                ["increment"] = increment,
                ["gotoNode"] = gotoNode
            };
        }
    }
}
