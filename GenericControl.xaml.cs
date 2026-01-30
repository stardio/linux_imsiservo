using System.Collections.Generic;
using System.Text.Json;
using System.Windows.Controls;

namespace EtherCAT_Studio
{
    public partial class GenericControl : UserControl
    {
        public GenericControl()
        {
            InitializeComponent();
        }

        public void Load(JsonElement? root)
        {
            // nothing to load
        }

        public Dictionary<string, object> Collect()
        {
            return new Dictionary<string, object>();
        }
    }
}