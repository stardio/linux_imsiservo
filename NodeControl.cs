using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EtherCAT_Studio
{
    // 노드 컨트롤 (포트 포함)
    public class NodeControl : Canvas
    {
        public Ellipse InputPort { get; }
        public Ellipse OutputPort { get; }
        public TextBlock Label { get; }
        public MainWindow Main { get; }
        bool _isDragging;
        Point _dragOffset;

        public MainWindow.PortInfo InputPortInfo { get; }
        public MainWindow.PortInfo OutputPortInfo { get; }

        public NodeControl(string text, Brush color, MainWindow main)
        {
            Main = main;
            Width = 140; Height = 48;

            // 배경
            var border = new Border
            {
                Width = 140, Height = 48,
                Background = color,
                CornerRadius = new CornerRadius(8),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2)
            };
            Children.Add(border);

            // 입력 포트
            InputPort = new Ellipse
            {
                Width = 16, Height = 16,
                Fill = Brushes.Gray,
                Stroke = Brushes.White,
                StrokeThickness = 1.5,
                Cursor = Cursors.Hand
            };
            SetLeft(InputPort, -8); SetTop(InputPort, 16);
            Children.Add(InputPort);

            // 출력 포트
            OutputPort = new Ellipse
            {
                Width = 16, Height = 16,
                Fill = Brushes.Gray,
                Stroke = Brushes.White,
                StrokeThickness = 1.5,
                Cursor = Cursors.Hand
            };
            SetLeft(OutputPort, 132); SetTop(OutputPort, 16);
            Children.Add(OutputPort);

            // 라벨
            Label = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Width = 120,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            SetLeft(Label, 10); SetTop(Label, 12);
            Children.Add(Label);

            // 포트 정보
            InputPortInfo = new MainWindow.PortInfo { Type = MainWindow.PortType.Input, Node = this, Ellipse = InputPort };
            OutputPortInfo = new MainWindow.PortInfo { Type = MainWindow.PortType.Output, Node = this, Ellipse = OutputPort };

            // 포트 하이라이트
            InputPort.MouseEnter += (s, e) => InputPort.Fill = Brushes.LimeGreen;
            InputPort.MouseLeave += (s, e) => InputPort.Fill = Brushes.Gray;
            OutputPort.MouseEnter += (s, e) => OutputPort.Fill = Brushes.Orange;
            OutputPort.MouseLeave += (s, e) => OutputPort.Fill = Brushes.Gray;

            // 와이어 드래그
            OutputPort.MouseLeftButtonDown += (s, e) => {
                Main.StartWireDrag(OutputPortInfo, e);
                e.Handled = true;
            };

            // 노드 드래그
            MouseLeftButtonDown += Node_MouseLeftButtonDown;
            MouseMove += Node_MouseMove;
            MouseLeftButtonUp += Node_MouseLeftButtonUp;
        }

        // 포트의 Canvas 내 실제 위치 반환
        public Point GetPortPosition(MainWindow.PortInfo port, Canvas canvas)
        {
            return port.Ellipse.TranslatePoint(new Point(port.Ellipse.Width / 2, port.Ellipse.Height / 2), canvas);
        }

        // 노드 드래그
        void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragOffset = e.GetPosition(this);
            CaptureMouse();
        }
        void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(Parent as Canvas);
                Canvas.SetLeft(this, pos.X - _dragOffset.X);
                Canvas.SetTop(this, pos.Y - _dragOffset.Y);
                Main.NodeMoved(this);
            }
        }
        void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            ReleaseMouseCapture();
        }

        // Update visuals after property edit
        public void UpdateVisuals()
        {
            // Update label and optionally color
            // (For now, just update label)
            // If you want to update color, add logic here
        }
    }
}
