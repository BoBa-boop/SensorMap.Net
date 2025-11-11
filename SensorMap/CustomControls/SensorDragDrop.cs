using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SensorMap.CustomControls
{

    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_Sensor", Type = typeof(Ellipse))]
    public class SensorDragDrop : Control
    {
        static SensorDragDrop()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SensorDragDrop),
                new FrameworkPropertyMetadata(typeof(SensorDragDrop)));
        }

        private Canvas _canvas;
        private Ellipse _sensor;
        private bool _isDragging = false;

        #region Dependency Properties
        #region managmentProps
        public static readonly DependencyProperty CanZoomProperty =
           DependencyProperty.Register("CanZoom", typeof(bool), typeof(SensorDragDrop), new PropertyMetadata(false));

        public bool CanZoom
        {
            get { return (bool)GetValue(CanZoomProperty); }
            set { SetValue(CanZoomProperty, value); }
        }
        public static readonly DependencyProperty CanPanningProperty =
           DependencyProperty.Register("CanPanning", typeof(bool), typeof(SensorDragDrop),new PropertyMetadata(false));

        public static readonly DependencyProperty ZoomFactorProp =
           DependencyProperty.Register("ZoomFactor", typeof(double), typeof(SensorDragDrop), new PropertyMetadata(1.1));

        public double ZoomFactor
        {
            get { return (double)GetValue(ZoomFactorProp); }
            set { SetValue(ZoomFactorProp, value); }
        }

        public bool CanPanning
        {
            get { return (bool)GetValue(CanPanningProperty); }
            set { SetValue(CanPanningProperty, value); }
        }
        #endregion
        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(SensorDragDrop),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(SensorDragDrop),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        public static readonly DependencyProperty SensorDropCommandProperty =
            DependencyProperty.Register("SensorDropCommand", typeof(ICommand), typeof(SensorDragDrop));

        public ICommand SensorDropCommand
        {
            get { return (ICommand)GetValue(SensorDropCommandProperty); }
            set { SetValue(SensorDropCommandProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(BitmapFrame), typeof(SensorDragDrop),
            new PropertyMetadata(default(BitmapFrame)));
        public BitmapFrame ImageSource
        {
            get { return (BitmapFrame)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        #endregion
        private readonly MatrixTransform _transform = new MatrixTransform();
        private Point _initialMousePosition;
        private UIElement _selectedElement;
        private Vector _draggingDelta;
        private double _scaleDelta;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _scaleDelta = ZoomFactor;
            _canvas = GetTemplateChild("PART_Canvas") as Canvas;
            _sensor = GetTemplateChild("PART_Sensor") as Ellipse;
            if (_canvas != null)
            {
                if (_sensor != null)
                {
                    _sensor.MouseLeftButtonUp += Canvas_Drop;
                    _sensor.MouseLeftButtonDown += _canvas_MouseMove;
                }
                _canvas.DragLeave += Canvas_DragLeave;
                _canvas.MouseMove += _canvas_MouseMove;
                _canvas.MouseDown += _canvas_MouseDown;
                _canvas.MouseWheel += _canvas_MouseWheel;//zoom
                _canvas.MouseUp += _canvas_MouseUp;
            }
        }

        private void _canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Rect bounds = VisualTreeHelper.GetDescendantBounds(_canvas); // Границы нашего объекта
            Size parentSize = RenderSize; // Размер родительского элемента
            if (e.RightButton == MouseButtonState.Pressed)
            {
                Point mousePosition = _transform.Inverse.Transform(e.GetPosition(_canvas));
                Vector delta = Point.Subtract(mousePosition, _initialMousePosition);



                //// Получаем координаты нового положения объекта
                double newX = _transform.Matrix.OffsetX;
                double newY = _transform.Matrix.OffsetY;

                if (bounds.Width > parentSize.Width)
                {
                    newX = _transform.Matrix.OffsetX + delta.X;
                    if (newX >= 0.0)
                    {
                        newX = 0.0;
                    }
                    else if (0.0 - newX + parentSize.Width >= bounds.Width)
                    {
                        newX = parentSize.Width - bounds.Width;
                    }
                }
                if (bounds.Height > parentSize.Height)
                {
                    newY = _transform.Matrix.OffsetY + delta.Y;
                    if (newY >= 0.0)
                    {
                        newY = 0.0;
                    }
                    else if (0.0 - newY + parentSize.Height >= bounds.Height)
                    {
                        newY = parentSize.Height - bounds.Width;
                    }

                }
                var translate = new TranslateTransform(newX, newY);
                _transform.Matrix = translate.Value; /** _transform.Matrix;*/

                foreach (UIElement child in _canvas.Children)
                {
                    child.RenderTransform = _transform;
                }
            }
        

            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                double x = Mouse.GetPosition(this).X;
                double y = Mouse.GetPosition(this).Y;

                if (_selectedElement != null)
                {
                    Canvas.SetLeft(_selectedElement, x + _draggingDelta.X);
                    Canvas.SetTop(_selectedElement, y + _draggingDelta.Y);
                }
            }
        }

        private void _canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _selectedElement = null;
        }

        private void _canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            double scaleFactor = ZoomFactor;
            if (e.Delta < 0)
            {
                scaleFactor = 1.0 / scaleFactor;
            }

            Point mousePostion = e.GetPosition(this);

            Matrix scaleMatrix = _transform.Matrix;
            scaleMatrix.ScaleAt(scaleFactor, scaleFactor, mousePostion.X, mousePostion.Y);
            _scaleDelta = _scaleDelta * scaleFactor;
            _transform.Matrix = scaleMatrix;

            foreach (UIElement child in _canvas.Children)
            {
                double x = Canvas.GetLeft(child);
                double y = Canvas.GetTop(child);

                double sx = Math.Round(x * scaleFactor,2);
                double sy = Math.Round(y * scaleFactor,2);

                Canvas.SetLeft(child, sx);
                Canvas.SetTop(child, sy);

                child.RenderTransform = _transform;
            }
        }

        private void _canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                _initialMousePosition = _transform.Inverse.Transform(e.GetPosition(this));
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                //перемещаем только объект
                if (_canvas.Children.Contains((UIElement)e.Source))
                {
                    _selectedElement = (UIElement)e.Source;
                    Point mousePosition = Mouse.GetPosition(this);
                    double x = Canvas.GetLeft(_selectedElement);
                    double y = Canvas.GetTop(_selectedElement);
                    Point elementPosition = new Point(x, y);
                    _draggingDelta = elementPosition - mousePosition;
                }
                _isDragging = true;
            }
        }

        private void Canvas_Drop(object sender, MouseButtonEventArgs e)
        {
            X = Math.Round(Canvas.GetLeft(_selectedElement) / _scaleDelta,1);
            Y = Math.Round(Canvas.GetTop(_selectedElement) / _scaleDelta, 1);
            SensorDropCommand?.Execute(null);
            e.Handled = true;
        }

        private void Canvas_DragLeave(object sender, DragEventArgs e)
        {
           
        }

        

        // Метод для обновления позиции сенсора извне
        public void SetPosition(double x, double y)
        {
            if (_sensor != null)
            {
                Canvas.SetLeft(_sensor, x - _sensor.ActualWidth / 2);
                Canvas.SetTop(_sensor, y - _sensor.ActualHeight / 2);
            }
        }
    }
}

