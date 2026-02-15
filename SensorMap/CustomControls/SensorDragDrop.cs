using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using SensorMap.Behaviors;
using SensorMap.Commands.SensorCommands;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using SensorMap.ViewModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Application = System.Windows.Application;
using Brushes = System.Windows.Media.Brushes;
using Control = System.Windows.Controls.Control;
using Cursor = System.Windows.Input.Cursor;
using Cursors = System.Windows.Input.Cursors;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using Image = System.Windows.Controls.Image;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

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
        
        private Canvas? _canvas;
        private Image? _image;

        #region Dependency Properties

        public bool IsEditMode
        {
            get { return (bool)GetValue(IsEditModeProperty); }
            set { SetValue(IsEditModeProperty, value); SetValue(CustomSensor.IsEditModeProperty, value); }
        }
        public static readonly DependencyProperty IsEditModeProperty =
            DependencyProperty.Register("IsEditMode", typeof(bool), typeof(SensorDragDrop), new PropertyMetadata(false));
        public bool IsShowAddresses
        {
            get { return (bool)GetValue(IsShowAddressesProperty); }
            set { SetValue(IsShowAddressesProperty, value); }
        }
        public static readonly DependencyProperty IsShowAddressesProperty =
            DependencyProperty.Register("IsShowAddresses", typeof(bool), typeof(SensorDragDrop), new PropertyMetadata(false));

        public static readonly DependencyProperty SaveSensorsCommandProperty =
            DependencyProperty.Register("SaveSensorsCommand", typeof(ICommand), typeof(SensorDragDrop));
        public ICommand SaveSensorsCommand
        {
            get { return (ICommand)GetValue(SaveSensorsCommandProperty); }
            set { SetValue(SaveSensorsCommandProperty, value); }
        }
        public static readonly DependencyProperty ShowAddressCommandProperty =
            DependencyProperty.Register("ShowAddress", typeof(ICommand), typeof(SensorDragDrop),new PropertyMetadata(null));

        public ICommand ShowAddressCommand
        {
            get { return (ICommand)GetValue(ShowAddressCommandProperty); }
            set { SetValue(ShowAddressCommandProperty, value); }
        }
        public static readonly DependencyProperty AddSensorsCommandProperty =
            DependencyProperty.Register("AddSensorsCommand", typeof(ICommand), typeof(SensorDragDrop), new PropertyMetadata(null));

        public ICommand AddSensorsCommand
        {
            get { return (ICommand)GetValue(AddSensorsCommandProperty); }
            set { SetValue(AddSensorsCommandProperty, value); }
        }
        
        public static readonly DependencyProperty CoordProperty = DependencyProperty.Register("Coord", typeof(Point), typeof(SensorDragDrop),
            new PropertyMetadata(default(Point)));
        public Point Coord
        {
            get { return (Point)GetValue(CoordProperty); }
            set { SetValue(CoordProperty, value); }
        }
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource",
            typeof(ObservableCollection<SensorAssignments>), typeof(SensorDragDrop),
            new PropertyMetadata(new ObservableCollection<SensorAssignments>(), OnItemsSourceChanged));

        public ObservableCollection<SensorAssignments> ItemsSource
        {
            get { return (ObservableCollection<SensorAssignments>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource",
            typeof(BitmapFrame), typeof(SensorDragDrop),
            new PropertyMetadata(default(BitmapFrame)));
        public BitmapFrame ImageSource
        {
            get { return (BitmapFrame)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }
        #endregion
        private ITransformObject _transformObject;
        private MatrixTransform? _viewMatrixTransform;
        private Matrix _viewMatrix = Matrix.Identity;
        private Point _initialMousePosition;
        private double scaleLevel = 1;
        private bool _isDropAdd = false;
        private Rect movingObject;  // Границы нашего объекта
        private Size parentSize; // Размер родительского элемента
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            _canvas = GetTemplateChild("PART_Canvas") as Canvas;
            _image = GetTemplateChild("PART_Image") as Image;
            _transformObject = new TransformObjectService();
            if (_canvas != null)
            {
                _viewMatrixTransform = new MatrixTransform(Matrix.Identity);
                _canvas.RenderTransform = _viewMatrixTransform;
                _viewMatrix = _viewMatrixTransform.Matrix;
                MapProperties.SetViewMatrix(this, _viewMatrix);

                _canvas.MouseMove += _canvas_MouseMove;
                _canvas.MouseDown += _canvas_MouseDown;
                _canvas.MouseUp += _canvas_MouseUp;
                _canvas.MouseWheel += _canvas_MouseWheel;
                _canvas.Drop += _canvas_Drop;

                Application.Current.MainWindow.PreviewMouseDown += OnMainWindowClick;

                void OnMainWindowClick(object sender, MouseButtonEventArgs e)
                {
                    foreach (var uiElement in _canvas.Children.OfType<CustomSensor>())
                    {
                        uiElement.IsSelected = false;
                    }
                }
                ShowAddressCommand = new RelayCommand(ShowAddressChanged);
            }
            
        }

        private void _canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _initialMousePosition = new Point();
            
        }

        private void ShowAddressChanged()
        {
            IsShowAddresses = !IsShowAddresses;
            foreach (var uiElement in _canvas!.Children.OfType<CustomSensor>())
            {
                uiElement.ShowAddresses = IsShowAddresses;
            }
            
        }
        #region ItemsSource events

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SensorDragDrop)d;
            if (control._canvas == null) return;
            control._viewMatrixTransform!.Matrix = Matrix.Identity;
            if (e.NewValue != null)
            {
                if (e.NewValue is INotifyCollectionChanged notify) notify.CollectionChanged += control.OnCollectionChanged;
                if (!e.NewValue.Equals(e.OldValue)) control.SourceCollectionChanged();
            }
        }

        private void SourceCollectionChanged()
        {
            if (_canvas == null) return;
            var nonSensorChildren = _canvas.Children.OfType<UIElement>()
                                    .Where(element=>element.GetType().Name!="CustomSensor").ToList();
            _canvas.Children.Clear();
            foreach (var element in nonSensorChildren)
            {
                _canvas.Children.Add(element);
            }
            foreach (var sensor in ItemsSource)
            {
                AddSensorToCanvas(sensor);
            }
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems == null) return;
                    foreach (SensorAssignments newItem in e.NewItems)
                    {
                        AddSensorToCanvas(newItem);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems == null) return;
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

        private void AddSensorToCanvas(SensorAssignments sensor)
        {
            int sensorsInMap = _canvas!.Children.OfType<CustomSensor>().Count();
            if (sensor!=null && !_isDropAdd && ItemsSource.Count != sensorsInMap)
            {
                sensor.X = sensor.X < 0 ? 50 : sensor.X;
                sensor.Y = sensor.Y < 0 ? 50 : sensor.Y;
                double offsetX, offsetY;
                GetLeftTopPoint(out offsetX, out offsetY);
                CustomSensor element = CreateSensorObject(sensor, new Point(sensor.X + Math.Abs(offsetX), sensor.Y + Math.Abs(offsetY)));
                var command = new AddSensor(sensor, element, _canvas, ItemsSource);
                AddSensorsCommand.Execute(command);
                element.Tag = ItemsSource.IndexOf(sensor);
            }
        }
        

        


        #endregion
        #region CanvasEvents
        private void _canvas_Drop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if (data is SensorAssignments sensorData&&IsEditMode)
            {  
                if (sensorData != null)
                {
                    Point dropPosition = e.GetPosition(_canvas);
                    CustomSensor element = CreateSensorObject(sensorData,_transformObject.WorldToScreen(dropPosition, _viewMatrix));
                    _isDropAdd = true;
                    var command = new AddSensor(sensorData, element, _canvas!, ItemsSource);
                    AddSensorsCommand.Execute(command);
                    element.Tag = ItemsSource.IndexOf(sensorData);

                }
            }
        }
        private void _canvas_MouseMove(object sender, MouseEventArgs e)
        {
            parentSize = RenderSize;
            if (e.RightButton == MouseButtonState.Pressed&&_initialMousePosition.X>0)
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
                    if (delta.X + _viewMatrixTransform!.Matrix.OffsetX >= 0.0)
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
                    if (delta.Y + _viewMatrixTransform!.Matrix.OffsetY >= 0.0)
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
                _viewMatrixTransform!.Matrix = translate.Value * _viewMatrixTransform.Matrix;

            }
            Coord = new Point(Math.Round(Mouse.GetPosition(_canvas).X,0), Math.Round(Mouse.GetPosition(_canvas).Y, 0));

        }
        #region Zoom
        private void _canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {            
            parentSize = RenderSize;
            Point mousePosition = e.GetPosition(this);
            double delta = e.Delta > 0 ? 1.08 : 1.0 / 1.08;
            
            Matrix scaleMatrix = _viewMatrixTransform!.Matrix;
            double newZoom = scaleMatrix.M11 * delta;

            // Проверка предела масштаба
            if (newZoom < 0.2 || newZoom > 10.0) return;

            Point mouseScreen = e.GetPosition(_canvas);
            Point mouseWorldBefore = _transformObject.ScreenToWorld(mouseScreen, _viewMatrix);

            
            double currentOffsetX = scaleMatrix.OffsetX;
            double currentOffsetY = scaleMatrix.OffsetY;

            //// Применяем масштабирование
            scaleMatrix.ScaleAt(delta, delta, mousePosition.X, mousePosition.Y);
            
            //// Вычисляем новые размеры изображения
            double scaledWidth = _image!.ActualWidth * newZoom;
            double scaledHeight = _image.ActualHeight * newZoom;


            ApplyBounds(ref scaleMatrix, scaledWidth, scaledHeight, parentSize, currentOffsetX, currentOffsetY, scaleLevel);
            _viewMatrixTransform.Matrix = scaleMatrix;
            foreach (UIElement wo in _canvas!.Children)
            {
                if (wo != null)
                {
                    if (IsUIElementSensor(wo, out CustomSensor element))
                    {
                        var sensor = ItemsSource.ElementAt((Int32)element.Tag);

                        Point screen = _transformObject.WorldToScreen(new Point(sensor.X, sensor.Y), _viewMatrix);
                        Canvas.SetLeft(wo, screen.X);
                        Canvas.SetTop(wo, screen.Y);
                    }
                }
            }
            MapProperties.SetViewMatrix(this, _viewMatrix);
            
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
        #endregion
        private void _canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                _initialMousePosition = e.GetPosition(_canvas);
                movingObject = VisualTreeHelper.GetDescendantBounds(this);
            }
        }

        private bool IsUIElementSensor(object UIElement,out CustomSensor sensor)
        {
            if (UIElement is CustomSensor brd && brd.Tag is int tagIndex && tagIndex >= 0 && tagIndex < ItemsSource.Count)
            {
                // Получаем соответствующий объект SensorAssignments по индексу
                var assignment = ItemsSource[(int)tagIndex];
                sensor = brd;
                return true;
            }
            else
            {
                sensor = default(CustomSensor)!;
                return false;
            }

        }
        #endregion
        private void UIElementSensor_ShowMoreInfo(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                if (IsUIElementSensor(sender,out CustomSensor element))
                {
                    var pop = new View.SensorAddInfo();
                    var window = new PopupWindow()
                    {
                        PopupElement = pop,
                        DataContext = element.SensorData
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
        private CustomSensor CreateSensorObject(SensorAssignments sensor, Point point)
        {           
            var element = new CustomSensor();
            element.SensorData = sensor;
            
            Canvas.SetLeft(element, point.X);
            Canvas.SetTop(element, point.Y);

            element.AddHandler(UIElement.MouseRightButtonDownEvent, new MouseButtonEventHandler(UIElementSensor_ShowMoreInfo), false);
            return element;
        }


       
        /// <summary>
        /// Получение точки верхнего левого угла
        /// </summary>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        private void GetLeftTopPoint(out double offsetX, out double offsetY)
        {            
            offsetX = _viewMatrixTransform!.Matrix.OffsetX;
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

