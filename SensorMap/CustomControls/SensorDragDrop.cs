using HandyControl.Controls;
using SensorMap.Commands.SensorCommands;
using SensorMap.Model;
using SensorMap.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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

        //private ScaleTransform _scaleTransform = new ScaleTransform();
        //private TranslateTransform _translateTransform = new TranslateTransform();
        //private TransformGroup _transformGroup = new TransformGroup();
        private double dOffsetX = 0;
        private double dOffsetY = 0;
        private double dStartPanX = 0;
        private double dStartPanY = 0;
        #region Dependency Properties

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
        public static readonly DependencyProperty CoordProperty = DependencyProperty.Register("Coord", typeof(Point), typeof(SensorDragDrop),
            new PropertyMetadata(default(Point)));
        public Point Coord
        {
            get { return (Point)GetValue(CoordProperty); }
            set { SetValue(CoordProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource",
            typeof(ObservableCollection<SensorAssignments>), typeof(SensorDragDrop), new PropertyMetadata(null, OnItemsSourceChanged));

        public ObservableCollection<SensorAssignments> ItemsSource
        {
            get { return (ObservableCollection<SensorAssignments>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        #endregion        
        private MatrixTransform _viewMatrixTransform;
        private Matrix _viewMatrix = Matrix.Identity;
        private Point _initialMousePosition;
        private UIElement _selectedElement;
        private SensorAssignments _selectedSensor = new();
        private double scaleLevel = 1;
        private Vector _draggingDelta;
        private bool _isDropAdd = false;
        private Rect movingObject;  // Границы нашего объекта
        private Size parentSize; // Размер родительского элемента
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _canvas = GetTemplateChild("PART_Canvas") as Canvas;
            _image = GetTemplateChild("PART_Image") as Image;

            if (_canvas != null)
            {
                _viewMatrixTransform = new MatrixTransform(Matrix.Identity);
                _canvas.RenderTransform = _viewMatrixTransform;
                _viewMatrix = _viewMatrixTransform.Matrix;

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
                case NotifyCollectionChangedAction.Reset:
                    //необходимо очищать коллекцию
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
            int sensorsInMap = 0;
            foreach (var item in _canvas.Children)
            {
                if (IsUIElementSensor(item, out Border element))
                    sensorsInMap++;
            }
            if (sensor!=null && !_isDropAdd && ItemsSource.Count != sensorsInMap)
            {
                sensor.X = sensor.X < 0 ? 50 : sensor.X;
                sensor.Y = sensor.Y < 0 ? 50 : sensor.Y;
                double offsetX, offsetY;
                GetLeftTopPoint(out offsetX, out offsetY);
                Border element = CreateSensorObject(sensor, new Point(sensor.X + Math.Abs(offsetX), sensor.Y + Math.Abs(offsetY)));
                UndoRedoStack.Do(new AddSensor(sensor, element, _canvas, ItemsSource));
                element.Tag = ItemsSource.IndexOf(sensor);
            }
        }
        

        private void SensorSelected_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.MiddleButton!=MouseButtonState.Pressed)
            {
            _selectedElement = (UIElement)e.Source;
                if (IsUIElementSensor(_selectedElement,out Border element))
                {
                    _selectedSensor = ItemsSource[Convert.ToInt32(element.Tag)];
                }
            }
        }


        #endregion

        #region SensorEvents
        private void Sensor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_selectedSensor.Sensor != null)
            {
                if(IsUIElementSensor(_selectedElement,out Border element))
                { 
                    // 1. Получаем текущие экранные координаты
                    double screenX = Canvas.GetLeft(_selectedElement);
                    double screenY = Canvas.GetTop(_selectedElement);

                    // 2. Конвертируем в мировые
                    Point worldPoint = ScreenToWorld(new Point(screenX, screenY));

                    // 5. Создаем команду с МИРОВЫМИ координатами
                    UndoRedoStack.Do(new MoveSensor(_selectedElement,worldPoint,_selectedSensor,(x) => WorldToScreen(worldPoint)
                    ));

                    element.BorderBrush = Brushes.Transparent;
                }
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
                    Border element = CreateSensorObject(sensorData,WorldToScreen(dropPosition));
                    _isDropAdd = true;
                    UndoRedoStack.Do(new AddSensor(sensorData, element, _canvas, ItemsSource));

                    element.Tag = ItemsSource.IndexOf(sensorData);

                }
            }
        }
        private void _canvas_MouseMove(object sender, MouseEventArgs e)
        {
            parentSize = RenderSize;
            if (e.RightButton == MouseButtonState.Pressed)
            {
                //запрет на перемещение
                if (movingObject.Width <= parentSize.Width || movingObject.Height <= parentSize.Height)
                {
                    return;
                }
                Point mousePosition = e.GetPosition(_canvas);
                mousePosition = new Point(Math.Round(mousePosition.X, 0), Math.Round(mousePosition.Y, 0));
                Point initMouseRounded = new Point(Math.Round(_initialMousePosition.X, 0), Math.Round(_initialMousePosition.Y, 0));
                Vector delta = Point.Subtract(mousePosition, initMouseRounded);
                //задание границ
                if (movingObject.Width > parentSize.Width)
                {
                    //левая граница
                    if (delta.X + _viewMatrixTransform.Matrix.OffsetX >= 0.0)
                    {
                        delta.X = 0.0;
                        _initialMousePosition = mousePosition;
                    }
                    //правая граница
                    else if (delta.X + _viewMatrixTransform.Matrix.OffsetX < parentSize.Width - movingObject.Width)
                    {
                        delta.X = parentSize.Width - movingObject.Width - _viewMatrixTransform.Matrix.OffsetX;
                        _initialMousePosition = mousePosition;
                    }
                }
                if (movingObject.Height > parentSize.Height)
                {
                    //верхняя граница
                    if (delta.Y + _viewMatrixTransform.Matrix.OffsetY >= 0.0)
                    {
                        delta.Y = 0.0;
                        _initialMousePosition = mousePosition;
                    }
                    //нижняя граница
                    else if (delta.Y + _viewMatrixTransform.Matrix.OffsetY < parentSize.Height - movingObject.Height)
                    {
                        delta.Y = parentSize.Height - movingObject.Height - _viewMatrixTransform.Matrix.OffsetY;
                        _initialMousePosition = mousePosition;
                    }

                }

                var translate = new TranslateTransform(delta.X, delta.Y);
                _viewMatrixTransform.Matrix = translate.Value * _viewMatrixTransform.Matrix;

            }
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                double x = Mouse.GetPosition(_canvas).X;
                double y = Mouse.GetPosition(_canvas).Y;
                
                if (IsUIElementSensor(_selectedElement,out Border element))
                {
                    Canvas.SetLeft(element, x + _draggingDelta.X);
                    Canvas.SetTop(element, y + _draggingDelta.Y);                    
                }
            }
            Coord = new Point(Math.Round(Mouse.GetPosition(_canvas).X,0), Math.Round(Mouse.GetPosition(_canvas).Y, 0));

        }

        private void _canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {            
            parentSize = RenderSize;
            Point mousePosition = e.GetPosition(this);
            double delta = e.Delta > 0 ? 1.08 : 1.0 / 1.08;
            
            Matrix scaleMatrix = _viewMatrixTransform.Matrix;
            double newZoom = scaleMatrix.M11 * delta;

            // Проверка предела масштаба
            if (newZoom < 0.2 || newZoom > 10.0) return;

            Point mouseScreen = e.GetPosition(_canvas);
            Point mouseWorldBefore = ScreenToWorld(mouseScreen);

            
            double currentOffsetX = scaleMatrix.OffsetX;
            double currentOffsetY = scaleMatrix.OffsetY;

            //// Применяем масштабирование
            scaleMatrix.ScaleAt(delta, delta, mousePosition.X, mousePosition.Y);
            
            //// Вычисляем новые размеры изображения
            double scaledWidth = _image.ActualWidth * newZoom;
            double scaledHeight = _image.ActualHeight * newZoom;


            ApplyBounds(ref scaleMatrix, scaledWidth, scaledHeight, parentSize, currentOffsetX, currentOffsetY, scaleLevel);
            _viewMatrixTransform.Matrix = scaleMatrix;
            foreach (UIElement wo in _canvas.Children)
            {
                if (wo != null)
                {
                    if (IsUIElementSensor(wo, out Border element))
                    {
                        var sensor = ItemsSource.ElementAt((Int32)element.Tag);

                        Point screen = WorldToScreen(new Point(sensor.X, sensor.Y));
                        Canvas.SetLeft(wo, screen.X);
                        Canvas.SetTop(wo, screen.Y);
                    }
                }
            }
            
        }
        
        /// <summary>
        /// Коррекция пустоты у границы
        /// </summary>
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
                _initialMousePosition = e.GetPosition(_canvas);
                movingObject = VisualTreeHelper.GetDescendantBounds(this);
            }
            if (e.ChangedButton == MouseButton.Left)
            {
                //перемещаем только датчик
                _selectedElement = (UIElement)e.Source;
                if (IsUIElementSensor(_selectedElement,out Border element))
                {
                    element.BorderBrush = Brushes.ForestGreen;
                    Point mousePosition = Mouse.GetPosition(_canvas);
                    double x = Canvas.GetLeft(element);
                    double y = Canvas.GetTop(element);
                    Point elementPosition = new Point(x, y);
                    _draggingDelta = elementPosition - mousePosition;
                    _isDragging = true;
                }
            }
        }

        private bool IsUIElementSensor(object UIElement,out Border element)
        {
            if(UIElement is Border brd && brd.Tag != null && ItemsSource.Contains(ItemsSource.ElementAt((Int32)brd.Tag)))
            {
                element = brd;
                return true;
            }
            else
            {
                element = new Border();
                return false;
            }
                
        }


        private void Canvas_DragLeave(object sender, DragEventArgs e)
        {
           
        }
        #endregion
        private void UIElementSensor_ShowMoreInfo(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                if (IsUIElementSensor(sender,out Border element))
                {
                    var pop = new View.SensorAddInfo();
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
                    window.Show(element, false);
                }
                e.Handled = true;
            }
        }
        private Border CreateSensorObject(SensorAssignments sensor, Point point)
        {           
            var element = new Border();
            element.CornerRadius = new CornerRadius(20);
            element.BorderThickness=new Thickness(1.5);
            element.Width = 40;
            element.Height = 40;
            element.Background = Brushes.Red;

            Canvas.SetLeft(element, point.X);
            Canvas.SetTop(element, point.Y);

            sensor.X = Math.Round(point.X,0); sensor.Y = Math.Round(point.Y,0);

            element.AddHandler(UIElement.MouseRightButtonDownEvent, new MouseButtonEventHandler(UIElementSensor_ShowMoreInfo), false);
            element.MouseLeftButtonUp += Sensor_MouseLeftButtonUp;
            element.PreviewMouseDown += SensorSelected_MouseDown;

            return element;
        }





        // 1. Мировые → Экранные
        private Point WorldToScreen(Point world)
        {
            double x = world.X;
            double y = world.Y;
            double sx = _viewMatrix.M11 * x + _viewMatrix.M12 * y + _viewMatrix.OffsetX;
            double sy = _viewMatrix.M21 * x + _viewMatrix.M22 * y + _viewMatrix.OffsetY;
            return new Point(sx, sy);
        }

        private Point ScreenToWorld(Point screen)
        {
            if (!_viewMatrix.HasInverse) return new Point(0, 0);
            Matrix inv = _viewMatrix;
            inv.Invert();
            double x = screen.X, y = screen.Y;
            double wx = inv.M11 * x + inv.M12 * y + inv.OffsetX;
            double wy = inv.M21 * x + inv.M22 * y + inv.OffsetY;
            return new Point(wx, wy);
        }
        /// <summary>
        /// Получение точки верхнего левого угла
        /// </summary>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        private void GetLeftTopPoint(out double offsetX, out double offsetY)
        {            
            offsetX = _viewMatrixTransform.Matrix.OffsetX;
            offsetY = _viewMatrixTransform.Matrix.OffsetY;
            if (_viewMatrixTransform.Matrix.M11 != 1)
            {
                offsetX /= _viewMatrixTransform.Matrix.M11;
                offsetY /= _viewMatrixTransform.Matrix.M22;
            }
            if (offsetX > 0 && offsetY < 0)
            {
                offsetX = 0;
                offsetY = _viewMatrixTransform.Matrix.OffsetY;
            }
            if (offsetY > 0 && offsetX < 0)
            {
                offsetY = 0;
                offsetX = _viewMatrixTransform.Matrix.OffsetX;
            }
        }

    }
}

