using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace WebMRecorder
{
    public partial class ScreenAreaSelectorWindow
    {
        private Rectangle _selectionRectangle;
        private Point _startPosition;
        private TranslateTransform _rectangleTextBlockTransform;

        public ScreenAreaSelectorWindow()
        {
            InitializeComponent();

            Height = SystemParameters.PrimaryScreenHeight;
            Width = SystemParameters.PrimaryScreenWidth;
            Left = 0;
            Top = 0;

            ScreenSelectorCanvas.MouseDown += ScreenSelectorCanvasOnMouseDown;
        }

        private void ScreenSelectorCanvasOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            ScreenSelectorCanvas.MouseDown -= ScreenSelectorCanvasOnMouseDown;
            ScreenSelectorCanvas.MouseMove += ScreenSelectorCanvasOnMouseMove;
            ScreenSelectorCanvas.MouseUp += ScreenSelectorCanvasOnMouseUp;

            _selectionRectangle = new Rectangle
            {
                StrokeThickness = 2,
                Stroke = new SolidColorBrush(Colors.Red),
                Fill= new SolidColorBrush(Colors.Transparent)
            };
            _startPosition = mouseButtonEventArgs.GetPosition(ScreenSelectorCanvas);

            _selectionRectangle.SetValue(LeftProperty, _startPosition.X);
            _selectionRectangle.SetValue(TopProperty, _startPosition.Y);
            _selectionRectangle.Width = 0;
            _selectionRectangle.Height = 0;

            ScreenSelectorCanvas.Children.Add(_selectionRectangle);
            _rectangleTextBlockTransform = new TranslateTransform();
            RectangleTextBlock.RenderTransform = _rectangleTextBlockTransform;
            RectangleTextBlock.Visibility = Visibility.Visible;
        }

        private void InvalidateTextBlock(Point coordinates)
        {
            var rect = GetSelectionRectangle();
            _rectangleTextBlockTransform.X = coordinates.X;
            _rectangleTextBlockTransform.Y = coordinates.Y;
            RectangleTextBlock.Text = $"x:{rect.X} y: {rect.Y} w: {rect.Width} h: {rect.Height}";
        }

        private void ScreenSelectorCanvasOnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {

            ScreenSelectorCanvas.MouseMove -= ScreenSelectorCanvasOnMouseMove;
            ScreenSelectorCanvas.MouseUp -= ScreenSelectorCanvasOnMouseUp;

            GetSelectionRectangle();
            Close();
        }

        public Rect GetSelectionRectangle()
        {
            var x = (double) _selectionRectangle.GetValue(LeftProperty);
            var y = (double) _selectionRectangle.GetValue(TopProperty);
            var width = (double) _selectionRectangle.GetValue(WidthProperty);
            var height = (double) _selectionRectangle.GetValue(HeightProperty);
            return new Rect(x, y, width, height);
        }

        private void ScreenSelectorCanvasOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            var movePosition = mouseEventArgs.GetPosition(ScreenSelectorCanvas);

            var positionDifference = new Point(_startPosition.X-movePosition.X, _startPosition.Y-movePosition.Y);

            if (positionDifference.X > 0)
            {
                _selectionRectangle.SetValue(LeftProperty, movePosition.X);
                _selectionRectangle.SetValue(WidthProperty, positionDifference.X);
            }
            else
            {
                _selectionRectangle.SetValue(WidthProperty, -positionDifference.X);
            }

            if (positionDifference.Y > 0)
            {
                _selectionRectangle.SetValue(TopProperty, movePosition.Y);
                _selectionRectangle.SetValue(HeightProperty, positionDifference.Y);
            }
            else
            {
                _selectionRectangle.SetValue(HeightProperty, -positionDifference.Y);
            }
            InvalidateTextBlock(movePosition);
        }
    }
}
