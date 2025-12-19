using HandyControl.Controls;
using SensorMap.Commands.SensorCommands;
using SensorMap.Model;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace SensorMap.CustomControls
{
    
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_Image", Type = typeof(Image))]
    public class SensorDragDrop : Control
    {
        static SensorDragDrop()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SensorDragDrop),
                new FrameworkPropertyMetadata(typeof(SensorDragDrop)));
           
        }
        private Canvas _canvas;
        private Image _image;
        private bool _isDragging = false;

        #region Dependency Properties

        #region managmentProps
        public static readonly DependencyProperty ZoomFactorProp =
           DependencyProperty.Register("ZoomFactor", typeof(double), typeof(SensorDragDrop), new PropertyMetadata(1.08));

        public double ZoomFactor
        {
            get { return (double)GetValue(ZoomFactorProp); }
            set { SetValue(ZoomFactorProp, value); }
        }
        #endregion

        public static readonly DependencyProperty UndoRedoStackProperty = DependencyProperty.Register("UndoRedoStack", typeof(UndoRedoStack),typeof(SensorDragDrop),
         new PropertyMetadata(null));

        public UndoRedoStack UndoRedoStack
        {
            get => (UndoRedoStack)GetValue(UndoRedoStackProperty);
            set => SetValue(UndoRedoStackProperty, value);
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

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource",
            typeof(ObservableCollection<SensorAssignments>), typeof(SensorDragDrop), new PropertyMetadata(null, OnItemsSourceChanged));

        public ObservableCollection<SensorAssignments> ItemsSource
        {
            get { return (ObservableCollection<SensorAssignments>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        #endregion
        private readonly MatrixTransform _transform = new MatrixTransform();
        private Point _initialMousePosition;
        private UIElement _selectedElement;
        private SensorAssignments _selectedSensor = new();
        private double scaleFactor;
        private Vector _draggingDelta;
        private bool _isDropAdd = false;
        private double _scaleDeltaSensor;
        private Rect movingObject;  // Границы нашего объекта
        private Size parentSize; // Размер родительского элемента
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            scaleFactor = ZoomFactor;
            _scaleDeltaSensor = ZoomFactor;
            _canvas = GetTemplateChild("PART_Canvas") as Canvas;
            _image = GetTemplateChild("PART_Image") as Image;
            if (_canvas != null)
            {
                _canvas.DragLeave += Canvas_DragLeave;
                _canvas.MouseMove += _canvas_MouseMove;
                _canvas.MouseDown += _canvas_MouseDown;
                _canvas.MouseWheel += _canvas_MouseWheel;
                _canvas.Drop += _canvas_Drop;
                
            }
        }


        #region ItemsSource events
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SensorDragDrop)d;
            control.UnsubscribeFromCollection(e.OldValue as ObservableCollection<SensorAssignments>);
            control.SubscribeToCollection(e.NewValue as ObservableCollection<SensorAssignments>);
        }

        private void SubscribeToCollection(ObservableCollection<SensorAssignments> collection)
        {
            if (collection != null)
            {
                collection.CollectionChanged += OnCollectionChanged;
            }
            
        }
        private void UnsubscribeFromCollection(ObservableCollection<SensorAssignments> collection)
        {
            if (collection != null)
            {
                collection.CollectionChanged -= OnCollectionChanged;
            }
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (SensorAssignments newItem in e.NewItems)
                    {
                        AddEllipseToCanvas(newItem);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (SensorAssignments oldItem in e.OldItems)
                    {
                        RemoveEllipseFromCanvas(oldItem);
                    }
                    break;
                
                
            }
            _isDropAdd = false;
        }
        #endregion

        #region SensorActionsLogic
        private void RemoveEllipseFromCanvas(SensorAssignments oldItem)
        {
            //if (_canvas == null) return;

            //var ellipseToRemove = _canvas.Children.

            //if (ellipseToRemove != null)
            //{
            //    _canvas.Children.Remove(ellipseToRemove);
            //}
            //отписаться от событий клика
        }

        private void AddEllipseToCanvas(SensorAssignments sensor)
        {
            int sensorsInMap = _canvas.Children.OfType<Ellipse>().Count();
            if (sensor!=null && !_isDropAdd&&ItemsSource.Count()!= sensorsInMap)
            {
                if (sensor.X < 0 && sensor.Y < 0)
                {
                    sensor.X = 50;
                    sensor.Y = 50; 
                }

                Ellipse element = CreateSensorObject(sensor, sensor.X, sensor.Y);
                UndoRedoStack.Do(new AddSensor(sensor, element, _canvas, ItemsSource));
                element.Tag = sensor.Id;
            }
        }

        private void SensorSelected_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.MiddleButton!=MouseButtonState.Pressed)
            {
            _selectedElement = (UIElement)e.Source;
                if (_selectedElement is Ellipse)
                {
                    if (_selectedElement is FrameworkElement frameworkElement)
                    {
                        int tag = Convert.ToInt32(frameworkElement.Tag);
                        if (tag != _selectedSensor.Id)
                            _selectedSensor = ItemsSource.Where(x => x.Id == tag).FirstOrDefault();
                    }
                }
            }
        }


        #endregion

        #region SensorEvents
        private void Sensor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_selectedSensor.Sensor != null)
            {
                double X = Math.Round(Canvas.GetLeft(_selectedElement), 2);
                double Y = Math.Round(Canvas.GetTop(_selectedElement), 2);
                //_selectedSensor.X = Math.Round(Canvas.GetLeft(_selectedElement), 2);
                //_selectedSensor.Y = Math.Round(Canvas.GetTop(_selectedElement), 2);
                UndoRedoStack.Do(new MoveSensor(_selectedSensor, _selectedElement, X,Y));
                ////SensorDropCommand?.Execute(_selectedSensor);
                e.Handled = true;
                _isDragging = false;
                _selectedElement = null;
                _selectedSensor = new SensorAssignments();
            }
        }
        #endregion


        #region CanvasEvents
        private void _canvas_Drop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if (data is SensorAssignments sensorData)
            {  
                if (sensorData != null)
                {
                    Point dropPosition = e.GetPosition(_canvas);
                    Ellipse element = CreateSensorObject(sensorData, dropPosition.X, dropPosition.Y);
                    _isDropAdd = true;
                    UndoRedoStack.Do(new AddSensor(sensorData, element, _canvas, ItemsSource));

                    element.Tag = sensorData.Id;

                }
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
                    x = x + _draggingDelta.X;
                    y = y + _draggingDelta.Y;
                }
            }
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
                //перемещаем только датчик
                _selectedElement = (UIElement)e.Source;
                if (_selectedElement is Ellipse)
                {
                    Point mousePosition = Mouse.GetPosition(_canvas);
                    double x = Canvas.GetLeft(_selectedElement);
                    double y = Canvas.GetTop(_selectedElement);
                    Point elementPosition = new Point(x, y);
                    _draggingDelta = elementPosition - mousePosition;
                    _isDragging = true;
                }
                else _selectedElement = null;
            }
        }

       

        private void Canvas_DragLeave(object sender, DragEventArgs e)
        {
           
        }
        #endregion
        private void Ellipse_ShowMoreInfo(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                var ellipse = sender as Ellipse;

            if (ellipse != null)
            {
                var pop = new View.test();
                var window = new PopupWindow()
                {
                    PopupElement = pop,
                    DataContext = _selectedSensor
                };
                Application.Current.MainWindow.PreviewMouseDown += OnMainWindowClick;

                void OnMainWindowClick(object sender, MouseButtonEventArgs e)
                {
                    window.Close();
                    Application.Current.MainWindow.PreviewMouseDown -= OnMainWindowClick;
                }
                window.Show(ellipse, false);
            }

                e.Handled = true;
            }
        }
        private Ellipse CreateSensorObject(SensorAssignments sensorData, double X,double Y)
        {           
            var element = new Ellipse();
            element.Width = 30;
            element.Height = 30;
            element.Fill = Brushes.Red;
            
            Canvas.SetLeft(element, X);
            Canvas.SetTop(element, Y);

            element.AddHandler(UIElement.MouseRightButtonDownEvent, new MouseButtonEventHandler(Ellipse_ShowMoreInfo), false);
            element.MouseLeftButtonUp += Sensor_MouseLeftButtonUp;
            element.PreviewMouseDown += SensorSelected_MouseDown;

            return element;
        }
    }
}

