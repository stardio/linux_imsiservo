using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EtherCAT_Studio
{
    public partial class MainWindow : Window
    {
        public enum PortType { Input, Output }

        public class PortInfo
        {
            public PortType Type { get; set; }
            public NodeControl? Node { get; set; }
            public Ellipse? Ellipse { get; set; }
        }

        // 연결 정보
        public class Connection
        {
            public PortInfo? From { get; set; }
            public PortInfo? To { get; set; }
            public Path? Path { get; set; }
        }

        private readonly List<Connection> _connections = new();
        private NodeControl? _selectedNode = null;
        private Connection? _selectedConnection = null;
        private PortInfo? _draggingFromPort = null;
        private Path? _tempWire = null;

        public MainWindow()
        {
            InitializeComponent();
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            MainCanvas.MouseLeftButtonDown += MainCanvas_MouseLeftButtonDown_ClearSelection;
        }

        private void MainCanvas_MouseLeftButtonDown_ClearSelection(object? sender, MouseButtonEventArgs e)
        {
            // click on canvas background clears selection
            if (e.Source == MainCanvas)
            {
                ClearSelection();
            }
        }

        public void SelectNode(NodeControl node)
        {
            ClearSelection();
            _selectedNode = node;
            _selectedNode.IsSelected = true;
        }

        public void SelectConnection(Connection conn)
        {
            ClearSelection();
            _selectedConnection = conn;
            if (conn.Path != null)
            {
                conn.Path.Stroke = Brushes.Cyan;
                conn.Path.StrokeThickness = 4;
            }
        }

        private void ClearSelection()
        {
            if (_selectedNode != null)
            {
                _selectedNode.IsSelected = false;
                _selectedNode = null;
            }
            if (_selectedConnection != null)
            {
                if (_selectedConnection.Path != null)
                {
                    _selectedConnection.Path.Stroke = Brushes.Yellow;
                    _selectedConnection.Path.StrokeThickness = 3;
                }
                _selectedConnection = null;
            }
        }

        private void MainWindow_PreviewKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (_selectedConnection != null)
                {
                    // remove path and connection
                    if (_selectedConnection.Path != null)
                        MainCanvas.Children.Remove(_selectedConnection.Path);
                    _connections.Remove(_selectedConnection);
                    _selectedConnection = null;
                    e.Handled = true;
                    return;
                }
                if (_selectedNode != null)
                {
                    // remove any connections attached to node
                    var toRemove = _connections.Where(c => c.From?.Node == _selectedNode || c.To?.Node == _selectedNode).ToList();
                    foreach (var c in toRemove)
                    {
                        if (c.Path != null) MainCanvas.Children.Remove(c.Path);
                        _connections.Remove(c);
                    }
                    // remove node
                    MainCanvas.Children.Remove(_selectedNode);
                    _selectedNode = null;
                    e.Handled = true;
                    return;
                }
            }
        }

        private void AddMotionNode_Click(object sender, RoutedEventArgs e)
        {
            AddNode("ABS MOVE", Brushes.CornflowerBlue);
        }

        private void AddWaitNode_Click(object sender, RoutedEventArgs e)
        {
            AddNode("WAIT", Brushes.Orange);
        }
        private void AddSetDo_Click(object sender, RoutedEventArgs e)
        {
            AddNode("SET DO", Brushes.LimeGreen);
        }

        private void AddCheckDi_Click(object sender, RoutedEventArgs e)
        {
            AddNode("CHECK DI", Brushes.Gold);
        }

        private void AddGoto_Click(object sender, RoutedEventArgs e)
        {
            AddNode("GOTO", Brushes.IndianRed);
        }

        private void AddJump_Click(object sender, RoutedEventArgs e)
        {
            AddNode("JUMP", Brushes.MediumPurple);
        }
        private void AddJumpImport_Click(object sender, RoutedEventArgs e)
        {
            // smaller outer size (half) and only input port on left; keep port size default (16)
            AddNode("JUMP IMPORT", Brushes.MediumPurple, 16, 70, 24, hasInput: true, hasOutput: false);
        }

        private void AddJumpExport_Click(object sender, RoutedEventArgs e)
        {
            // smaller outer size (half) and only output port on right; keep port size default (16)
            AddNode("JUMP EXPORT", Brushes.MediumPurple, 16, 70, 24, hasInput: false, hasOutput: true);
        }

        private void AddNode(string text, Brush color, double portSize = 16, double width = 140, double height = 48, bool hasInput = true, bool hasOutput = true)
        {
            // NodeControl 인스턴스 생성 (색상 전달)
            var node = new NodeControl(text, color, this, portSize, width, height, hasInput, hasOutput);

            // 캔버스에 임의의 위치(계단식)로 배치
            double offset = 20 + (MainCanvas.Children.Count * 20);
            Canvas.SetLeft(node, offset);
            Canvas.SetTop(node, offset);

            MainCanvas.Children.Add(node);
        }

        public void StartWireDrag(PortInfo port, MouseButtonEventArgs e)
        {
            if (port.Type != PortType.Output) return;
            _draggingFromPort = port;

            // 임시 와이어 Path 생성
            _tempWire = new Path
            {
                Stroke = Brushes.Yellow,
                StrokeThickness = 3,
                StrokeDashArray = new DoubleCollection { 2, 2 },
                IsHitTestVisible = false
            };
            MainCanvas.Children.Add(_tempWire);

            MainCanvas.MouseMove += MainCanvas_MouseMove;
            MainCanvas.MouseLeftButtonUp += MainCanvas_MouseLeftButtonUp;
        }

        private void MainCanvas_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_draggingFromPort == null || _tempWire == null) return;
            if (_draggingFromPort.Node != null && _tempWire != null)
            {
                var start = _draggingFromPort.Node.GetPortPosition(_draggingFromPort, MainCanvas);
                var end = e.GetPosition(MainCanvas);
                _tempWire.Data = MakeBezierGeometry(start, end);
            }
        }

        private void MainCanvas_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
        {
            if (_draggingFromPort == null || _tempWire == null) return;

            // 입력 포트 위에 있으면 연결
            var pos = e.GetPosition(MainCanvas);
            foreach (var child in MainCanvas.Children)
            {
                if (child is NodeControl node)
                {
                    var inputPort = node.InputPortInfo;
                    var portPos = node.GetPortPosition(inputPort, MainCanvas);
                    if ((portPos - pos).Length < 16)
                    {
                        // 연결 생성
                        var conn = new Connection
                        {
                            From = _draggingFromPort,
                            To = inputPort,
                            Path = new Path
                            {
                                Stroke = Brushes.Yellow,
                                StrokeThickness = 3
                            }
                        };
                        if (conn.Path != null)
                        {
                            // make path clickable for selection
                            conn.Path.IsHitTestVisible = true;
                            conn.Path.MouseLeftButtonDown += (s, args) => { SelectConnection(conn); args.Handled = true; };
                            MainCanvas.Children.Add(conn.Path);
                        }
                        _connections.Add(conn);
                        UpdateConnectionPath(conn);
                        break;
                    }
                }
            }

            // 임시 와이어 제거
            MainCanvas.Children.Remove(_tempWire);
            _tempWire = null;
            _draggingFromPort = null;
            MainCanvas.MouseMove -= MainCanvas_MouseMove;
            MainCanvas.MouseLeftButtonUp -= MainCanvas_MouseLeftButtonUp;
        }

        private PathGeometry MakeBezierGeometry(Point start, Point end)
        {
            var geom = new PathGeometry();
            var seg = new BezierSegment
            {
                Point1 = new Point(start.X + 60, start.Y),
                Point2 = new Point(end.X - 60, end.Y),
                Point3 = end
            };
            var fig = new PathFigure { StartPoint = start, Segments = { seg } };
            geom.Figures.Add(fig);
            return geom;
        }

        private void UpdateConnectionPath(Connection conn)
        {
            if (conn.From?.Node != null && conn.To?.Node != null && conn.Path != null)
            {
                var start = conn.From.Node.GetPortPosition(conn.From, MainCanvas);
                var end = conn.To.Node.GetPortPosition(conn.To, MainCanvas);
                conn.Path.Data = MakeBezierGeometry(start, end);
            }
        }

        public void NodeMoved(NodeControl node)
        {
            // 노드 이동 시 연결된 와이어 업데이트
            foreach (var conn in _connections)
            {
                if ((conn.From?.Node == node) || (conn.To?.Node == node))
                    UpdateConnectionPath(conn);
            }
        }
        // 저장 버튼 클릭 시 모든 노드를 시퀀스 JSON 구조로 파일로 저장
        private void SaveJson_Click(object sender, RoutedEventArgs e)
        {
            var steps = new List<object>();
            int idx = 1;
            var nodes = new List<NodeControl>();
            foreach (var child in MainCanvas.Children)
            {
                if (child is NodeControl node)
                    nodes.Add(node);
            }
            if (nodes.Count == 0)
            {
                MessageBox.Show("저장할 노드가 없습니다.");
                return;
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                string id = $"node_{idx:00}";
                idx++;
                string type = (node.NodeType ?? "").ToUpperInvariant();
                type = type switch
                {
                    "MOTION" => "MOTION",
                    "M" => "MOTION",
                    "ABS MOVE" => "MOTION",
                    "WAIT" => "WAIT",
                    "IO" => "IO",
                    "SET DO" => "IO",
                    "CHECK DI" => "IO",
                    "FLOW" => "FLOW",
                    "GOTO" => "FLOW",
                    "JUMP" => "GOTO",
                    "JUMP IMPORT" => "GOTO",
                    "JUMP EXPORT" => "GOTO",
                    "SYSTEM" => "SYSTEM",
                    _ => type
                };

                object paramsObj = new { };
                if (!string.IsNullOrEmpty(node.JsonData))
                {
                        try
                        {
                            var doc = System.Text.Json.JsonDocument.Parse(node.JsonData);
                            var root = doc.RootElement;

                            static string GetPropString(System.Text.Json.JsonElement el, string name, string def)
                            {
                                if (!el.TryGetProperty(name, out var p)) return def;
                                if (p.ValueKind == System.Text.Json.JsonValueKind.String)
                                    return p.GetString() ?? def;
                                var raw = p.GetRawText();
                                return string.IsNullOrWhiteSpace(raw) ? def : raw.Trim('"');
                            }

                            static double GetPropDouble(System.Text.Json.JsonElement el, string name, double def)
                            {
                                if (!el.TryGetProperty(name, out var p)) return def;
                                if (p.ValueKind == System.Text.Json.JsonValueKind.Number && p.TryGetDouble(out var d)) return d;
                                if (p.ValueKind == System.Text.Json.JsonValueKind.String && double.TryParse(p.GetString(), out var d2)) return d2;
                                return def;
                            }

                            if (type == "MOTION")
                            {
                                string axis = GetPropString(root, "axis", "X");
                                double speed = GetPropDouble(root, "speed", 0);
                                double pos = GetPropDouble(root, "position", GetPropDouble(root, "pos", 0));
                                paramsObj = new { axis, pos, speed };
                            }
                            else if (type == "WAIT")
                            {
                                int delay = (int)GetPropDouble(root, "delay_ms", GetPropDouble(root, "delay", 0));
                                paramsObj = new { delay_ms = delay };
                            }
                            else
                            {
                                if (root.TryGetProperty("params", out var p))
                                {
                                    paramsObj = System.Text.Json.JsonSerializer.Deserialize<object>(p.GetRawText()) ?? new { };
                                }
                            }
                                // Handle jump/goto params (jump_target -> label)
                                if (type == "GOTO")
                                {
                                    if (root.TryGetProperty("jump_target", out var jt) && jt.ValueKind == System.Text.Json.JsonValueKind.String)
                                    {
                                        paramsObj = new { label = jt.GetString() };
                                    }
                                    else if (root.TryGetProperty("label", out var lbl) && lbl.ValueKind == System.Text.Json.JsonValueKind.String)
                                    {
                                        paramsObj = new { label = lbl.GetString() };
                                    }
                                }
                        }
                        catch { }
                }

                string next = (i < nodes.Count - 1) ? $"node_{(i + 2):00}" : null;
                var step = new Dictionary<string, object?>
                {
                    ["id"] = id,
                    ["type"] = type,
                    ["params"] = paramsObj,
                    ["next"] = next
                };
                steps.Add(step);
            }

            var rootObj = new Dictionary<string, object>
            {
                ["sequence_name"] = "Sequence",
                ["steps"] = steps
            };

            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            string outJson = System.Text.Json.JsonSerializer.Serialize(rootObj, options);

            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON 파일 (*.json)|*.json|모든 파일 (*.*)|*.*",
                FileName = "sequence.json"
            };
            if (dlg.ShowDialog() == true)
            {
                System.IO.File.WriteAllText(dlg.FileName, outJson);
                MessageBox.Show("저장 완료: " + dlg.FileName);
            }
        }
    }
}