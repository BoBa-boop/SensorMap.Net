using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Tools;
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
using System.Xml.Linq;

namespace SensorMap.CustomControls
{
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_Sensor", Type = typeof(Ellipse))]
    [TemplatePart(Name = "PART_Image", Type = typeof(Image))]
    public class SensorDragDrop : Control
    {
        static SensorDragDrop()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SensorDragDrop),
                new FrameworkPropertyMetadata(typeof(SensorDragDrop)));
        }
        private Canvas _canvas;
        private Ellipse _sensor;
        private Image _image;
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
           DependencyProperty.Register("ZoomFactor", typeof(double), typeof(SensorDragDrop), new PropertyMetadata(1.08));

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
        private double scaleFactor;
        private Vector _draggingDelta;
        private double _scaleDeltaSensor;
        private Rect movingObject;  // Границы нашего объекта
        private Size parentSize; // Размер родительского элемента
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            scaleFactor = ZoomFactor;
            _scaleDeltaSensor = ZoomFactor;
            _canvas = GetTemplateChild("PART_Canvas") as Canvas;
            _sensor = GetTemplateChild("PART_Sensor") as Ellipse;
            _image = GetTemplateChild("PART_Image") as Image;

            
            
            if (_canvas != null)
            {
                if (_sensor != null)
                {
                    _sensor.MouseLeftButtonUp += Canvas_Drop;
                }
                _canvas.DragLeave += Canvas_DragLeave;
                _canvas.MouseMove += _canvas_MouseMove;
                _canvas.MouseDown += _canvas_MouseDown;
                _canvas.MouseWheel += _canvas_MouseWheel;//zoom
                _canvas.MouseUp += _canvas_MouseUp;
                _canvas.Drop += _canvas_Drop;
            }
        }

        private void _canvas_Drop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            
            if(data is UIElement element)
            {
                Point dropPosition = e.GetPosition(_canvas);
                Canvas.SetLeft(element, dropPosition.X);
                Canvas.SetTop(element, dropPosition.Y);

                _canvas.Children.Add(element);
            }
        }

        private void _canvas_MouseMove(object sender, MouseEventArgs e)
        {
            parentSize = RenderSize;
            if (e.RightButton == MouseButtonState.Pressed)
            {
                //запрет на перемещение
                if(movingObject.Width <= parentSize.Width || movingObject.Height <= parentSize.Height)
                {
                    return;
                }
                Point mousePosition = _transform.Inverse.Transform(e.GetPosition(_canvas));
                Vector delta = Point.Subtract(mousePosition, _initialMousePosition);
                //задание границ
                if (movingObject.Width > parentSize.Width)
                {
                    //левая граница
                    if (delta.X + _transform.Matrix.OffsetX >= 0.0)
                    {
                        delta.X = 0.0;
                        _initialMousePosition = mousePosition;
                    }
                    //правая граница
                    else if (delta.X +  _transform.Matrix.OffsetX < parentSize.Width - movingObject.Width)
                    {
                        delta.X = parentSize.Width - movingObject.Width - _transform.Matrix.OffsetX;
                        _initialMousePosition = mousePosition;
                    }
                }
                if (movingObject.Height > parentSize.Height)
                { 
                    //верхняя граница
                    if (delta.Y + _transform.Matrix.OffsetY >= 0.0)
                    {
                        delta.Y = 0.0;
                        _initialMousePosition = mousePosition;
                    }
                    //нижняя граница
                    else if (delta.Y + _transform.Matrix.OffsetY < parentSize.Height - movingObject.Height)
                    {
                        delta.Y = parentSize.Height - movingObject.Height - _transform.Matrix.OffsetY;
                        _initialMousePosition = mousePosition;
                    }

                }

                var translate = new TranslateTransform(delta.X, delta.Y);
                _transform.Matrix = translate.Value * _transform.Matrix;


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
            parentSize = RenderSize;
            Point mousePosition = e.GetPosition(this);
            scaleFactor = ZoomFactor;
            if (e.Delta < 0)
            {
                scaleFactor = 1.0 / scaleFactor;
            }

            Matrix scaleMatrix = _transform.Matrix;
            double newScale = scaleMatrix.M11 * scaleFactor;

            // Проверка предела масштаба
            if (newScale < 0.5 || newScale > 10.0) return;

            // Сохраняем текущее состояние перед масштабированием
            double currentScale = scaleMatrix.M11;
            double currentOffsetX = scaleMatrix.OffsetX;
            double currentOffsetY = scaleMatrix.OffsetY;

            // Применяем масштабирование
            scaleMatrix.ScaleAt(scaleFactor, scaleFactor, mousePosition.X, mousePosition.Y);

            // Вычисляем новые размеры изображения
            double scaledWidth = _image.ActualWidth * newScale;
            double scaledHeight = _image.ActualHeight * newScale;

            // Применяем границы только при уменьшении
            if (e.Delta < 0) // Уменьшение
            {
                ApplyBounds(ref scaleMatrix, scaledWidth, scaledHeight, parentSize, currentOffsetX, currentOffsetY, scaleFactor);
            }

            _scaleDeltaSensor = _scaleDeltaSensor * scaleFactor;
            _transform.Matrix = scaleMatrix;

            foreach (UIElement child in _canvas.Children)
            {
                double x = Canvas.GetLeft(child);
                double y = Canvas.GetTop(child);

                double sx = Math.Round(x * scaleFactor, 2);
                double sy = Math.Round(y * scaleFactor, 2);

                Canvas.SetLeft(child, sx);
                Canvas.SetTop(child, sy);

                child.RenderTransform = _transform;
            }
            
        }

        private void ApplyBounds(ref Matrix matrix, double scaledWidth, double scaledHeight, Size parentSize,
                                double prevOffsetX, double prevOffsetY, double scaleFactor)
        {
            double offsetX = matrix.OffsetX;
            double offsetY = matrix.OffsetY;

            // Если изображение меньше контейнера по ширине - центрируем по горизонтали
            if (scaledWidth <= parentSize.Width)
            {
                offsetX = (parentSize.Width - scaledWidth) / 2;
            }
            else
            {
                // Проверяем левую границу
                if (offsetX > 0)
                    offsetX = 0;

                // Проверяем правую границу
                double minOffsetX = parentSize.Width - scaledWidth;
                if (offsetX < minOffsetX)
                    offsetX = minOffsetX;
            }

            // Если изображение меньше контейнера по высоте - центрируем по вертикали
            if (scaledHeight <= parentSize.Height)
            {
                offsetY = (parentSize.Height - scaledHeight) / 2;
            }
            else
            {
                // Проверяем верхнюю границу
                if (offsetY > 0)
                    offsetY = 0;

                // Проверяем нижнюю границу
                double minOffsetY = parentSize.Height - scaledHeight;
                if (offsetY < minOffsetY)
                    offsetY = minOffsetY;
            }

            // Устанавливаем скорректированные смещения
            matrix.OffsetX = offsetX;
            matrix.OffsetY = offsetY;
        }

        private void _canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                _initialMousePosition = _transform.Inverse.Transform(e.GetPosition(this));
                movingObject = VisualTreeHelper.GetDescendantBounds(this);
                
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                //перемещаем только объект
                if (_canvas.Children.Contains((UIElement)e.Source))
                {
                    _selectedElement = (UIElement)e.Source;
                    Point mousePosition = Mouse.GetPosition(_canvas);
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
            X = Math.Round(Canvas.GetLeft(_selectedElement) / _scaleDeltaSensor, 1);
            Y = Math.Round(Canvas.GetTop(_selectedElement) / _scaleDeltaSensor, 1);
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

