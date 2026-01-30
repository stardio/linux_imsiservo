using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace EtherCAT_Studio
{
    public partial class PropertyWindow : Window
    {
        public string? JsonResult { get; private set; }
        private string _nodeType;

        public PropertyWindow(string? initialJson = null, string nodeType = "")
        {
            InitializeComponent();
            _nodeType = nodeType;
            System.Diagnostics.Debug.WriteLine($"PropertyWindow initialized with nodeType: {_nodeType}");
            // Log nodeType to a file for debugging (use LocalApplicationData for writable location)
            try
            {
                var dir = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "EtherCAT_Studio");
                System.IO.Directory.CreateDirectory(dir);
                string logFilePath = System.IO.Path.Combine(dir, "debug_log.txt");
                System.IO.File.AppendAllText(logFilePath, $"PropertyWindow initialized with nodeType: {_nodeType}\n");
                System.Diagnostics.Debug.WriteLine($"WROTE LOG: {logFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write debug log: {ex.Message}");
            }

            // node-specific control will be created below and initialized from JSON if provided
            // host appropriate control
            UserControl nodeControl = (_nodeType ?? "").ToUpperInvariant() switch
            {
                "LINEAR_MOVE" => new LinearMoveControl(),
                "MOTION" => new MotionControl(),
                _ => new GenericControl()
            };
            NodeSpecificContent.Content = nodeControl;

            // Debug log which control was loaded for which nodeType
            try
            {
                var dir = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "EtherCAT_Studio");
                System.IO.Directory.CreateDirectory(dir);
                string logFilePath = System.IO.Path.Combine(dir, "debug_log.txt");
                string ctrlName = nodeControl.GetType().Name;
                string entry = $"PropertyWindow nodeType: {_nodeType} -> loaded control: {ctrlName}\n";
                System.IO.File.AppendAllText(logFilePath, entry);
                System.Diagnostics.Debug.WriteLine(entry);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write debug log (load control): {ex.Message}");
            }

            // If we have initial JSON, pass full root to control's Load
            if (!string.IsNullOrEmpty(initialJson))
            {
                try
                {
                    var doc = System.Text.Json.JsonDocument.Parse(initialJson);
                    var root = doc.RootElement;
                    // call Load if available
                    switch (nodeControl)
                    {
                        case LinearMoveControl lmc:
                            lmc.Load(root);
                            break;
                        case MotionControl mc2:
                            mc2.Load(root);
                            break;
                        case GenericControl gc:
                            gc.Load(root);
                            break;
                    }
                }
                catch { }
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            var obj = new Dictionary<string, object>();
            string label = LabelBox.Text ?? string.Empty;
            string jumpTarget = JumpTargetBox.Text ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(label)) obj["label"] = label;
            if (!string.IsNullOrWhiteSpace(jumpTarget)) obj["jump_target"] = jumpTarget;
            // collect node-specific parameters from the hosted control
            var hosted = NodeSpecificContent.Content;
            if (hosted is LinearMoveControl lmc)
            {
                foreach (var kv in lmc.Collect()) obj[kv.Key] = kv.Value;
            }
            else if (hosted is MotionControl mc)
            {
                foreach (var kv in mc.Collect()) obj[kv.Key] = kv.Value;
            }
            else if (hosted is GenericControl gc)
            {
                foreach (var kv in gc.Collect()) obj[kv.Key] = kv.Value;
            }
            JsonResult = System.Text.Json.JsonSerializer.Serialize(obj, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // node-specific UI is hosted inside `NodeSpecificContent` as UserControls
    }
}