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
        private PortInfo? _draggingFromPort = null;
        private Path? _tempWire = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddMotionNode_Click(object sender, RoutedEventArgs e)
        {
            AddNode("Motion", Brushes.CornflowerBlue);
        }

        private void AddWaitNode_Click(object sender, RoutedEventArgs e)
        {
            AddNode("Wait", Brushes.Orange);
        }

        private void AddIONode_Click(object sender, RoutedEventArgs e)
        {
            AddNode("IO", Brushes.LimeGreen);
        }

        private void AddFlowNode_Click(object sender, RoutedEventArgs e)
        {
            AddNode("Flow", Brushes.IndianRed);
        }

        private void AddSystemNode_Click(object sender, RoutedEventArgs e)
        {
            AddNode("System", Brushes.Gray);
        }

        private void AddNode(string text, Brush color)
        {
            // NodeControl 인스턴스 생성 (색상 전달)
            var node = new NodeControl(text, color, this);

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
    }
}