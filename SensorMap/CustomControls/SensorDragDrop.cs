using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using ReactiveUI;
using SensorMap.Behaviors;
using SensorMap.Commands.SensorCommands;
using SensorMap.EF;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Properties;
using SensorMap.Services;
using SensorMap.View;
using SensorMap.ViewModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
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
    /// <summary>
    /// Элементы добавляются на 0 слой, при выделение они поднимаются на 1 слой
    /// </summary>
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_Image", Type = typeof(Image))]
    public class SensorDragDrop : Control
    {
        static Cursor Grab = new Cursor(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/cursors/Grab.cur")).Stream);
        static Cursor Grabbing = new Cursor(Application.GetResourceStream(new Uri("pack://application:,,,/Resources/cursors/Grabbing.cur")).Stream);
        static SensorDragDrop()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SensorDragDrop),
                new FrameworkPropertyMetadata(typeof(SensorDragDrop)));
           
        }
        
        private Canvas? _canvas;
        private Image? _image;

        #region Dependency Properties


        public bool IsMultiSelection
        {
            get { return (bool)GetValue(IsMultiSelectionProperty); }
            set { SetValue(IsMultiSelectionProperty, value); }
        }
        public static readonly DependencyProperty IsMultiSelectionProperty =
            DependencyProperty.Register("IsMultiSelection", typeof(bool), typeof(SensorDragDrop), new PropertyMetadata(false));


        public bool IsEditMode
        {
            get { return (bool)GetValue(IsEditModeProperty); }
            set { SetValue(IsEditModeProperty, value); SetValue(CustomSensor.IsEditModeProperty, value); }
        }
        public static readonly DependencyProperty IsEditModeProperty =
            DependencyProperty.Register("IsEditMode", typeof(bool), typeof(SensorDragDrop), new PropertyMetadata(false));
        public bool IsHideAddresses
        {
            get { return (bool)GetValue(IsHideAddressesProperty); }
            set { SetValue(IsHideAddressesProperty, value); }
        }
        public static readonly DependencyProperty IsHideAddressesProperty =
            DependencyProperty.Register("IsHideAddresses", typeof(bool), typeof(SensorDragDrop), new PropertyMetadata(true));


        /// <summary>
        /// Срабатывает от выбора из списка датчика CustomSensor и передает в MechanismVM
        /// </summary>
        public SensorAssignments SelectedCustomSensor
        {
            get { return (SensorAssignments)GetValue(SelectedCustomSensorProperty); }
            set { SetValue(SelectedCustomSensorProperty, value); }
        }

        public static readonly DependencyProperty SelectedCustomSensorProperty =
            DependencyProperty.Register("SelectedCustomSensor", typeof(SensorAssignments), typeof(SensorDragDrop),new PropertyMetadata(null,SelectedChanged));

        private static void SelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SensorDragDrop)d;
            SensorAssignments sensor = (SensorAssignments)e.NewValue;
            SensorAssignments oldSensor = (SensorAssignments)e.OldValue;
            if (control != null && control._canvas != null)
            {
                var elementToUnSelect = control._canvas.Children
                    .OfType<CustomSensor>()
                    .FirstOrDefault(x => x.SensorData == oldSensor);

                var elementToSelect = control._canvas.Children
                    .OfType<CustomSensor>()
                    .FirstOrDefault(x => x.SensorData == sensor);

                elementToSelect?.SetCurrentValue(CustomSensor.IsSelectedProperty, true);
                elementToUnSelect?.SetCurrentValue(CustomSensor.IsSelectedProperty, false);
            }
        }



        public ICommand TransformSensorsCommand
        {
            get { return (ICommand)GetValue(TransformSensorsCommandProperty); }
            set { SetValue(TransformSensorsCommandProperty, value); }
        }
        public static readonly DependencyProperty TransformSensorsCommandProperty =
            DependencyProperty.Register("TransformSensorsCommand", typeof(ICommand), typeof(SensorDragDrop), new PropertyMetadata(null));




        public bool IsCustomSensorDragging
        {
            get { return (bool)GetValue(IsCustomSensorDraggingProperty); }
            set { SetValue(IsCustomSensorDraggingProperty, value); }
        }
        public static readonly DependencyProperty IsCustomSensorDraggingProperty =
            DependencyProperty.Register("IsCustomSensorDragging", typeof(bool), typeof(SensorDragDrop), new PropertyMetadata(false));



        public bool IsSelectionRectEnable
        {
            get { return (bool)GetValue(IsSelectionRectEnableProperty); }
            set { SetValue(IsSelectionRectEnableProperty, value); }
        }

        public static readonly DependencyProperty IsSelectionRectEnableProperty =
            DependencyProperty.Register("IsSelectionRectEnable", typeof(bool), typeof(SensorDragDrop), new PropertyMetadata(false));




        


        #region add-remove sensor

        public static readonly DependencyProperty SaveSensorsCommandProperty =
            DependencyProperty.Register("SaveSensorsCommand", typeof(ICommand), typeof(SensorDragDrop));
        public ICommand SaveSensorsCommand
        {
            get { return (ICommand)GetValue(SaveSensorsCommandProperty); }
            set { SetValue(SaveSensorsCommandProperty, value); }
        }

        public static readonly DependencyProperty AddSensorsCommandProperty =
            DependencyProperty.Register("AddSensorsCommand", typeof(ICommand), typeof(SensorDragDrop), new PropertyMetadata(null));

        public ICommand AddSensorsCommand
        {
            get { return (ICommand)GetValue(AddSensorsCommandProperty); }
            set { SetValue(AddSensorsCommandProperty, value); }
        }

        public static readonly DependencyProperty RemoveSensorEventProperty =
           DependencyProperty.Register("RemoveSensorEvent", typeof(ICommand), typeof(SensorDragDrop), new PropertyMetadata(null));
        
        /// <summary>
        /// Event срабатывает от CustomSensor события PreviewKeyDown
        /// </summary>
        public ICommand RemoveSensorEvent
        {
            get { return (ICommand)GetValue(RemoveSensorEventProperty); }
            set { SetValue(RemoveSensorEventProperty, value); }
        }

        public static readonly DependencyProperty RemoveSensorCommandProperty =
           DependencyProperty.Register("RemoveSensorCommand", typeof(ICommand), typeof(SensorDragDrop), new PropertyMetadata(null));
        /// <summary>
        /// Команда выполняет команду из MechanismVM
        /// </summary>
        public ICommand RemoveSensorCommand
        {
            get { return (ICommand)GetValue(RemoveSensorCommandProperty); }
            set { SetValue(RemoveSensorCommandProperty, value); }
        }
        public ICommand CopySensorsCommand
        {
            get { return (ICommand)GetValue(CopySensorsProperty); }
            set { SetValue(CopySensorsProperty, value); }
        }

        public static readonly DependencyProperty CopySensorsProperty =
            DependencyProperty.Register("CopySensorsCommand", typeof(ICommand), typeof(SensorDragDrop), new PropertyMetadata(null));
        public ICommand PasteSensorsCommand
        {
            get { return (ICommand)GetValue(PasteSensorsProperty); }
            set { SetValue(PasteSensorsProperty, value); }
        }

        public static readonly DependencyProperty PasteSensorsProperty =
            DependencyProperty.Register("PasteSensorsCommand", typeof(ICommand), typeof(SensorDragDrop), new PropertyMetadata(null));



        public ICommand CutSensorsCommand
        {
            get { return (ICommand)GetValue(CutSensorsProperty); }
            set { SetValue(CutSensorsProperty, value); }
        }   
        public static readonly DependencyProperty CutSensorsProperty =
            DependencyProperty.Register("CutSensorsCommand", typeof(ICommand), typeof(SensorDragDrop), new PropertyMetadata(null));


        #endregion

        #region coordOutput
        public static readonly DependencyProperty CoordProperty = DependencyProperty.Register("Coord", typeof(Point), typeof(SensorDragDrop),
            new PropertyMetadata(default(Point)));
        public Point Coord
        {
            get { return (Point)GetValue(CoordProperty); }
            set { SetValue(CoordProperty, value); }
        }
        #endregion

        #region Source Props
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

        #endregion

        private ITransformObject _transformObject;
        private IClipboard _clipboard;
        private List<SensorAssignments> tempSelectedSensorsCollection = new List<SensorAssignments>();
        private System.Windows.Shapes.Rectangle SelectionRect;
        private MatrixTransform? _viewMatrixTransform;
        private Matrix _viewMatrix = Matrix.Identity;
        private Point _initialMousePosition;
        private double scaleLevel = 1;
        private bool _isDropAdd = false;
        private bool _isDragging;
        private Rect movingObject;  // Границы нашего объекта
        private Size parentSize; // Размер родительского элемента
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            _canvas = GetTemplateChild("PART_Canvas") as Canvas;
            _image = GetTemplateChild("PART_Image") as Image;
            _transformObject = new TransformObjectService();
            _clipboard = new ClipboardService();
            CopySensorsCommand = new RelayCommand(CopySensors);
            PasteSensorsCommand = new RelayCommand(PasteSensors);
            CutSensorsCommand = new RelayCommand(CutSensors);
            RemoveSensorCommand = new RelayCommand(RemoveSensor);
            if (_canvas != null)
            {
                Cursor = Grab;
                _viewMatrixTransform = new MatrixTransform(Matrix.Identity);
                _canvas.RenderTransform = _viewMatrixTransform;
                _viewMatrix = _viewMatrixTransform.Matrix;
                MapProperties.SetViewMatrix(this, _viewMatrix);
                _canvas.PreviewMouseMove += _canvas_MouseMove;
                _canvas.MouseDown += _canvas_MouseDown;
                _canvas.MouseUp += _canvas_MouseUp;
                _canvas.MouseWheel += _canvas_MouseWheel;
                _canvas.Drop += _canvas_Drop;
                _canvas.MouseLeave += _canvas_MouseLeave;
                //_canvas.KeyDown += _canvas_KeyDown;
                //_canvas.KeyUp += _canvas_KeyUp;
                _image.PreviewMouseDown += OnMainWindowClick;
                void OnMainWindowClick(object sender, MouseButtonEventArgs e)
                {
                    foreach (var uiElement in _canvas.Children.OfType<CustomSensor>().Where(x=>x.IsSelected))
                    {
                        uiElement.IsSelected = false;
                        Canvas.SetZIndex(uiElement, 0);
                    }
                    tempSelectedSensorsCollection.Clear();
                    IsMultiSelection = false;
                }
                
            }
            
        }

        private void _canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            
        }

        //private void _canvas_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    if (e.Key == Key.LeftShift && _canvas.Children.OfType<CustomSensor>().Where(x=>x.IsSelected == true).Count()<2)
        //    {
        //        IsMultiSelection = false;
        //    }
        //}

        //private void _canvas_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    if((e.IsToggled) && e.Key == Key.LeftShift)
        //    {
        //        IsMultiSelection = true;
        //    }
        //}

        private void _canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _initialMousePosition = new Point();
            Mouse.OverrideCursor = null;
            Cursor = Grab;
            _canvas.Children.Remove(SelectionRect);
            TransformCommand(e);
            SelectionRect = new System.Windows.Shapes.Rectangle();
            IsSelectionRectEnable = false;
            _isDragging = false;
            Mouse.Capture(null);
        }

        private void TransformCommand(MouseButtonEventArgs e)
        {
            if(_isDragging)
            {
                Point worldPoint = _transformObject.ScreenToWorld(e.GetPosition(_canvas), MapProperties.GetViewMatrix(this));
                var canvasCollection = _canvas.Children.OfType<CustomSensor>().Where(x => tempSelectedSensorsCollection.Contains(x.SensorData)).ToList();
                var MultiTransform = new TransformationSensors(tempSelectedSensorsCollection,canvasCollection,
                    (x) => _transformObject.WorldToScreen(worldPoint, MapProperties.GetViewMatrix(this)));
                TransformSensorsCommand.Execute(MultiTransform);
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
        /// <summary>
        /// Изменение всей коллекции. Не сохраняется в Do/Undo
        /// </summary>
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
                int sensorsInMap = _canvas!.Children.OfType<CustomSensor>().Count();
                if (sensor != null && !_isDropAdd && ItemsSource.Count != sensorsInMap)
                {
                    sensor.X = sensor.X < 0 ? 50 : sensor.X;
                    sensor.Y = sensor.Y < 0 ? 50 : sensor.Y;
                    
                    CustomSensor element = CreateSensorObject(sensor, new Point(sensor.X,sensor.Y));
                    _canvas.Children.Add(element);
                    
                    element.Tag = sensor.Id;
                }
            }
        }
        /// <summary>
        /// Изменение коллекции при Добавление нового объекта.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            }
            _isDropAdd = false;
        }
        #endregion

        #region SensorActionsLogic
        public void RemoveSensor()
        {
            if (_canvas != null)
            {
                var SelectedSensorsUI = _canvas.Children.OfType<CustomSensor>().Where(x => x.IsSelected).ToList();

                var command = new RemoveSensor(SelectedSensorsUI, _canvas, ItemsSource);
                var param = new object[] { command, SelectedSensorsUI};
                RemoveSensorEvent.Execute(param);
                tempSelectedSensorsCollection.Clear();
                IsMultiSelection = false;
            }
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
                Canvas.SetZIndex(element, 0);
                element.Tag = sensor.Id;
            }
        }
        
        private void CopySensors()
        {
            _clipboard.Copy<List<CustomSensor>>(_canvas.Children.OfType<CustomSensor>().Where(x=>x.IsSelected).ToList());
        }
        private void PasteSensors()
        {
            var collection = _clipboard.Paste<List<CustomSensor>>();

            if (collection != null&&collection.Count>0)
            {
                foreach (var uiElement in _canvas.Children.OfType<CustomSensor>().Where(x=>x.IsSelected))
                {
                    uiElement.IsSelected = false;
                }
                tempSelectedSensorsCollection.Clear();
                foreach (var item in collection)
                {
                    var newSensor = (SensorAssignments)item.SensorData.Clone();
                    newSensor.Id = _canvas.Children.OfType<CustomSensor>().OrderBy(x => x.SensorData.Id).Last().SensorData.Id+1;
                    item.Tag = newSensor.Id;
                    newSensor.IsNew = true;
                    newSensor.Description = "Копия";
                    newSensor.Address = string.Empty;
                    ItemsSource.Add(newSensor);
                    var uiSensor = _canvas.Children.OfType<CustomSensor>()
                                   .Where(x => x.SensorData == newSensor).FirstOrDefault();
                    uiSensor.CustomBackground = Brushes.AliceBlue;
                    uiSensor.SetCurrentValue(CustomSensor.IsSelectedProperty, true);
                    tempSelectedSensorsCollection.Add(newSensor);
                }
            }
            
        }
        private void CutSensors()
        {
            List<CustomSensor> SelectedSensors = _canvas.Children.OfType<CustomSensor>().Where(x => x.IsSelected).ToList();
            _clipboard.Copy(SelectedSensors);
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
                    
                    element.Tag = sensorData.Id;
                }
            }
        }
        
        private void _canvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePosition = e.GetPosition(_canvas);
            if (e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Pressed && _initialMousePosition.X > 0)
            {

                if (_viewMatrixTransform == null) return;

                movingObject = VisualTreeHelper.GetDescendantBounds(this);
                bool CanMoveX = false;
                bool CanMoveY = false;
                double DeltaX = mousePosition.X - _initialMousePosition.X;
                double DeltaY = mousePosition.Y - _initialMousePosition.Y;
                double leftMargin = _viewMatrixTransform.Matrix.OffsetX;
                double topMargin = _viewMatrixTransform.Matrix.OffsetY;


                double scaledWidth = _image!.ActualWidth * _viewMatrixTransform.Matrix.M11;
                double scaledHeight = _image.ActualHeight * _viewMatrixTransform.Matrix.M11;

                //задание границ
                //левая граница
                if (movingObject.Width > parentSize.Width)
                {
                    leftMargin += DeltaX;
                    if (leftMargin > 0.0)
                    {
                        DeltaX = 0.0;
                    }
                    //правая граница
                    double minOffsetX = parentSize.Width - scaledWidth;
                    if (leftMargin < minOffsetX)
                        DeltaX = 0.0;
                    CanMoveX = true;
                }

                if (movingObject.Height > parentSize.Height)
                {
                    topMargin += DeltaY;
                    //верхняя граница
                    if (topMargin > 0.0)
                    {
                        DeltaY = 0.0;
                    }
                    //нижняя граница
                    double minOffsetY = parentSize.Height - scaledHeight;
                    if (topMargin < minOffsetY)
                        DeltaY = 0.0;

                    CanMoveY = true;
                }

                if (CanMoveX || CanMoveY)
                {
                    if ((CanMoveX == false))
                    {
                        DeltaX = 0;
                    }
                    if (CanMoveY == false)
                    {
                        DeltaY = 0;
                    }
                    Cursor = Grabbing;
                    var translate = new TranslateTransform(DeltaX, DeltaY);
                    _viewMatrixTransform!.Matrix = translate.Value * _viewMatrixTransform.Matrix;
                }
            }
            if (e.LeftButton == MouseButtonState.Pressed && IsCustomSensorDragging == false && _initialMousePosition.X > 0 && IsEditMode)
            {
                UpdateSectionRectangle(mousePosition);
            }
            IsGrabOrSelectNotActive(e);
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

            scaleMatrix.ScaleAt(delta, delta, mousePosition.X, mousePosition.Y);
            
            //// Вычисляем новые размеры изображения
            double scaledWidth = _image!.ActualWidth * newZoom;
            double scaledHeight = _image.ActualHeight * newZoom;


            ApplyBounds(ref scaleMatrix, scaledWidth, scaledHeight, parentSize);
            _viewMatrixTransform.Matrix = scaleMatrix;
            foreach (UIElement element in _canvas!.Children)
            {
                if (element != null)
                {
                    if (element is CustomSensor sensor1)
                    {
                        var sensor = ItemsSource.Where(x=>x == sensor1.SensorData).FirstOrDefault();
                        if (sensor == null) continue;
                        Point screen = _transformObject.WorldToScreen(new Point(Canvas.GetLeft(sensor1), Canvas.GetTop(sensor1)), _viewMatrix);
                        Canvas.SetLeft(sensor1, screen.X);
                        Canvas.SetTop(sensor1, screen.Y);
                    }
                }
            }
            MapProperties.SetViewMatrix(this, _viewMatrix);
        }
        
        /// <summary>
        /// Коррекция пустоты у границы
        /// </summary>
        private void ApplyBounds(ref Matrix matrix, double scaledWidth, double scaledHeight, Size parentSize)
        {
            double offsetX = matrix.OffsetX;
            double offsetY = matrix.OffsetY;
            double minOffsetY = parentSize.Height;
            double minOffsetX = parentSize.Width;
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
                minOffsetX = parentSize.Width - scaledWidth;
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
                minOffsetY = parentSize.Height - scaledHeight;
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
            _initialMousePosition = e.GetPosition(_canvas);
            //перемещение по карте
            if (e.ChangedButton == MouseButton.Right)
            {
                parentSize = RenderSize;
            }
            //выделение элементов на карте
            if (IsEditMode && e.ChangedButton == MouseButton.Left && IsCustomSensorDragging == false && IsMultiSelection == false)
            {
                Mouse.OverrideCursor = Cursors.Cross;//переопределяем курсор, чтобы не было конфликтов с CustomSensor
                CreateSelectionRectangle();
                IsSelectionRectEnable = true;
                Mouse.Capture(_canvas);
            }
        }
        #endregion
        private void UIElementSensor_ShowMoreInfo(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                if (sender is CustomSensor sensor)
                {
                    var pop = new View.SensorAddInfo();
                    var window = new PopupWindow()
                    {
                        PopupElement = pop,
                        DataContext = sensor.SensorData
                    };
                    Application.Current.MainWindow.PreviewMouseDown += OnMainWindowClick;

                    void OnMainWindowClick(object sender, MouseButtonEventArgs e)
                    {
                        window.Close();
                        Application.Current.MainWindow.PreviewMouseDown -= OnMainWindowClick;
                    }
                        window.Show(sensor, false);
                    }
                }
                e.Handled = true;
            
        }
        private CustomSensor CreateSensorObject(SensorAssignments sensor, Point point)
        {           
            var element = new CustomSensor();
            element.SensorData = sensor;
            element.CustomBackground = (SolidColorBrush)(new BrushConverter().ConvertFrom(sensor.Sensor.SensorType.Color??Colors.PaleVioletRed.ToString()));
            element.Focus();
            Canvas.SetLeft(element, point.X);
            Canvas.SetTop(element, point.Y);
            element.SensorData.X = point.X;
            element.SensorData.Y = point.Y;
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
        /// <summary>
        /// Получить датчики находящиеся в диапазоне выделения
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        private List<SensorAssignments> GetSensorsInSelectionRectangle(Point pos)
        {
            if (SelectionRect.Width > 5 && SelectionRect.Height > 5)
            {
                double left = Math.Min(_initialMousePosition.X, pos.X);
                double right = Math.Max(_initialMousePosition.X, pos.X);
                double top = Math.Min(_initialMousePosition.Y, pos.Y);
                double bottom = Math.Max(_initialMousePosition.Y, pos.Y);

                var colletionSensors = (from sensor in ItemsSource
                                        where !(sensor.X + sensor.Width < left ||
                                              sensor.X > right ||
                                              sensor.Y + sensor.Height < top ||
                                              sensor.Y > bottom)
                                        select sensor).ToList();
                foreach (var sensor in colletionSensors)
                {
                    if (_canvas != null)
                    {
                        var elementToSelect = _canvas.Children
                        .OfType<CustomSensor>()
                        .FirstOrDefault(x => x.SensorData == sensor);
                        if(elementToSelect != null)
                        {
                            elementToSelect.SetCurrentValue(CustomSensor.IsSelectedProperty, true);
                            elementToSelect.IsSelected = true;
                            Canvas.SetZIndex(elementToSelect, 1);
                        }
                    }
                }
                foreach (var sensor in tempSelectedSensorsCollection.Except(colletionSensors))
                {
                    if (_canvas != null)
                    {
                        var elementToSelect = _canvas.Children
                            .OfType<CustomSensor>()
                            .FirstOrDefault(x => x.SensorData == sensor);
                        if (elementToSelect != null)
                        {
                            elementToSelect.SetCurrentValue(CustomSensor.IsSelectedProperty, false);
                            elementToSelect.IsSelected = false;
                            Canvas.SetZIndex(elementToSelect, 0);
                        }
                    }
                }

                return new List<SensorAssignments>(colletionSensors);
            }
            return new List<SensorAssignments>();
        }
        /// <summary>
        /// Создать прямоугольник выделения
        /// </summary>
        private void CreateSelectionRectangle()
        {
            SelectionRect = new System.Windows.Shapes.Rectangle
            {
                Width = 0,
                Height = 0,
                Stroke = Brushes.Blue,
                StrokeDashArray = new DoubleCollection { 4, 2 },//пунктир
                Fill = new SolidColorBrush { Color = Colors.LightCyan, Opacity = 0.4 },
                StrokeThickness = 1.5
            };
            Canvas.SetLeft(SelectionRect, _initialMousePosition.X);
            Canvas.SetTop(SelectionRect, _initialMousePosition.Y);
            _canvas.Children.Add(SelectionRect);
        }
        /// <summary>
        /// Обновлять прямоугольник выделения
        /// </summary>
        /// <param name="pos"></param>
        private void UpdateSectionRectangle(Point pos)
        {
            double left = Math.Min(pos.X, _initialMousePosition.X);
            double top = Math.Min(pos.Y, _initialMousePosition.Y);
            double width = Math.Abs(pos.X - _initialMousePosition.X);
            double height = Math.Abs(pos.Y - _initialMousePosition.Y);

            Rect imageBounds = GetImageBounds();

            // Ограничиваем прямоугольник границами Image
            Rect constrainedRect = new Rect(left, top, width, height);
            constrainedRect.Intersect(imageBounds);

            SelectionRect.Width = constrainedRect.Width;
            SelectionRect.Height = constrainedRect.Height;
            Canvas.SetLeft(SelectionRect, constrainedRect.Left);
            Canvas.SetTop(SelectionRect, constrainedRect.Top);
            
            tempSelectedSensorsCollection = GetSensorsInSelectionRectangle(pos);
            IsMultiSelection = tempSelectedSensorsCollection.Count > 1;
        }
        private Rect GetImageBounds()
        {
            if (_image == null) return new Rect(0, 0, _canvas.ActualWidth, _canvas.ActualHeight);

            double left = Canvas.GetLeft(_image);
            double top = Canvas.GetTop(_image);

            // Если координаты не заданы, считаем что Image в (0,0)
            if (double.IsNaN(left)) left = 0;
            if (double.IsNaN(top)) top = 0;

            return new Rect(left, top, _image.ActualWidth, _image.ActualHeight);
        }
        /// <summary>
        /// Проверка что при выходе из-за границ SensorDragDrop отжаты ккнопки мыши и происходит перетаскивание или выделение. 
        /// Если верно, то сбрасывается pan и/или выделение.
        /// </summary>
        /// <param name="e"></param>
        private void IsGrabOrSelectNotActive(MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Released && e.RightButton == MouseButtonState.Released)
            {
                if (Cursor == Grabbing)
                {
                    _initialMousePosition = new Point();
                }
                if (Cursor == Cursors.Cross)
                {
                    _canvas.Children.Remove(SelectionRect);
                    SelectionRect = new System.Windows.Shapes.Rectangle();
                }
                Cursor = Grab;
            }
        }

    }
}

