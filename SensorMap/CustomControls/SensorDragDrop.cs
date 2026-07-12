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
        /// Выбранный объект на карте (датчик или устройство)
        /// </summary>
        public MapObject SelectedMapObject
        {
            get { return (MapObject)GetValue(SelectedMapObjectProperty); }
            set { SetValue(SelectedMapObjectProperty, value); }
        }

        public static readonly DependencyProperty SelectedMapObjectProperty =
            DependencyProperty.Register("SelectedMapObject", typeof(MapObject), typeof(SensorDragDrop), new PropertyMetadata(null, SelectedChanged));

        private static void SelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SensorDragDrop)d;
            MapObject newMapObj = e.NewValue as MapObject;
            MapObject oldMapObj = e.OldValue as MapObject;
            if (control != null && control._canvas != null)
            {
                if (control.IsMultiSelection)
                {
                    control.ClearSelectedSensors();
                }
                if (newMapObj == null)
                {
                    control.ClearSelectedSensors();
                }
                var elementToUnSelect = FindMapElement(control._canvas, oldMapObj);
                var elementToSelect = FindMapElement(control._canvas, newMapObj);

                if (elementToSelect is CustomSensor sensorToSelect)
                    sensorToSelect.SetCurrentValue(CustomSensor.IsSelectedProperty, true);
                else if (elementToSelect is CustomDevice deviceToSelect)
                    deviceToSelect.SetCurrentValue(CustomDevice.IsSelectedProperty, true);

                if (elementToUnSelect is CustomSensor sensorToUnSelect)
                    sensorToUnSelect.SetCurrentValue(CustomSensor.IsSelectedProperty, false);
                else if (elementToUnSelect is CustomDevice deviceToUnSelect)
                    deviceToUnSelect.SetCurrentValue(CustomDevice.IsSelectedProperty, false);
            }
        }
        private static FrameworkElement? FindMapElement(Canvas canvas, MapObject? mapData)
        {
            if (mapData == null) return null;
            return canvas.Children.OfType<FrameworkElement>()
                .FirstOrDefault(e => e is IMapElement me && me.MapData == mapData);
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
            typeof(ObservableCollection<MapObject>), typeof(SensorDragDrop),
            new PropertyMetadata(new ObservableCollection<MapObject>(), OnItemsSourceChanged));

        public ObservableCollection<MapObject> ItemsSource
        {
            get { return (ObservableCollection<MapObject>)GetValue(ItemsSourceProperty); }
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
        private List<MapObject> tempSelectedMapObjects = new List<MapObject>();
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
                _image.PreviewMouseDown += (s,e)=>ClearSelectedSensors();
                
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
                var canvasCollection = GetAllMapElements()
                    .Where(x => tempSelectedMapObjects.Contains(x.MapData)).ToList().Cast<IMapElement>().ToList();
                var MultiTransform = new TransformationSensors(tempSelectedMapObjects, canvasCollection,
                    (x) => _transformObject.WorldToScreen(worldPoint, MapProperties.GetViewMatrix(this)));
                TransformSensorsCommand.Execute(MultiTransform);
            }
        }
        /// <summary>
         /// Получить все UI-элементы карты (датчики и устройства)
         /// </summary>
        private IEnumerable<IMapElement> GetAllMapElements()
        {
            return _canvas.Children.OfType<IMapElement>();
        }

        /// <summary>
        /// Получить все UI-элементы карты как UIElement
        /// </summary>
        private IEnumerable<UIElement> GetAllMapUIElements()
        {
            return _canvas.Children.OfType<UIElement>().Where(e => e is IMapElement);
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
            var nonSensorChildren = _canvas.Children.OfType<FrameworkElement>()
                                    .Where(element=> element is not IMapElement).ToList();
            _canvas.Children.Clear();
            foreach (var element in nonSensorChildren)
            {
                _canvas.Children.Add(element);
            }
            foreach (var mapObj in ItemsSource)
            {
                int MapObjectsCanvas = GetAllMapElements().Count();
                if (mapObj != null && !_isDropAdd && mapObj.ToDelete==false && ItemsSource.Count != MapObjectsCanvas)
                {
                    mapObj.X = mapObj.X < 0 ? 50 : mapObj.X;
                    mapObj.Y = mapObj.Y < 0 ? 50 : mapObj.Y;
                    
                    FrameworkElement element = CreateMapObject(mapObj, new Point(mapObj.X,mapObj.Y));
                    _canvas.Children.Add(element);

                    if (element is FrameworkElement fe) fe.Tag = mapObj.Id;
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
                    foreach (MapObject newItem in e.NewItems)
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
                var selectedElements = GetAllMapElements()
                    .Where(e => e is IMapElement me && IsElementSelected(me))
                    .ToList();

                var command = new RemoveSensor(selectedElements, _canvas, ItemsSource);
                var param = new object[] { command, selectedElements };
                RemoveSensorEvent.Execute(param);
                tempSelectedMapObjects.Clear();
                IsMultiSelection = false;
            }
        }
        private bool IsElementSelected(IMapElement element)
        {
            if (element is CustomSensor s) return s.IsSelected;
            if (element is CustomDevice d) return d.IsSelected;
            return false;
        }

        private void SetElementSelected(IMapElement element, bool selected)
        {
            if (element is CustomSensor s) s.IsSelected = selected;
            else if (element is CustomDevice d) d.IsSelected = selected;
        }
        private void AddSensorToCanvas(MapObject mapObj)
        {
            int objectsInMap = GetAllMapElements().Count();
            if (mapObj != null && !_isDropAdd && ItemsSource.Count != objectsInMap)
            {
                mapObj.X = mapObj.X < 0 ? 50 : mapObj.X;
                mapObj.Y = mapObj.Y < 0 ? 50 : mapObj.Y;
                double offsetX, offsetY;
                GetLeftTopPoint(out offsetX, out offsetY);
                if (mapObj.Id == 0)
                {
                    var existingIds = GetAllMapElements().Select(e => e.MapData.Id).ToList();
                    mapObj.Id = existingIds.Any() ? existingIds.Max() + 1 : 1;
                }
                FrameworkElement element = CreateMapObject(mapObj, new Point(mapObj.X + Math.Abs(offsetX), mapObj.Y + Math.Abs(offsetY)));
                var command = new AddSensor(mapObj, (FrameworkElement)element, _canvas, ItemsSource);
                AddSensorsCommand.Execute(command);
                Canvas.SetZIndex(element, 0);
                if (element is FrameworkElement fe1) fe1.Tag = mapObj.Id;
            }
        }
        
        private void CopySensors()
        {
            var selectedElements = GetAllMapElements().Where(x => IsElementSelected(x)).ToList();
            _clipboard.Copy<List<IMapElement>>(selectedElements);
        }
        private void PasteSensors()
        {
            var collection = _clipboard.Paste<List<IMapElement>>();

            if (collection != null&&collection.Count>0)
            {
                foreach (var me in GetAllMapElements().Where(x => IsElementSelected(x)))
                {
                    SetElementSelected(me, false);
                }
                tempSelectedMapObjects.Clear();
                foreach (var item in collection)
                {
                    var newObj = (MapObject)item.MapData.Clone();
                    var existingIds = GetAllMapElements().Select(e => e.MapData.Id).ToList();
                    newObj.Id = existingIds.Any() ? existingIds.Max() + 1 : 1;
                    if (item is FrameworkElement feItem) feItem.Tag = newObj.Id;
                    newObj.IsNew = true;
                    newObj.Description = "Копия";
                    if (newObj is SensorAssignments sa) sa.Address = string.Empty;
                    ItemsSource.Add(newObj);
                    var uiElement = GetAllMapElements()
                                   .FirstOrDefault(x => x.MapData == newObj);
                    if (uiElement != null)
                    {
                        if (uiElement is CustomSensor cs)
                        {
                            cs.CustomBackground = Brushes.AliceBlue;
                            cs.SetCurrentValue(CustomSensor.IsSelectedProperty, true);
                        }
                        else if (uiElement is CustomDevice cd)
                        {
                            cd.CustomBackground = Brushes.AliceBlue;
                            cd.SetCurrentValue(CustomDevice.IsSelectedProperty, true);
                        }
                    }
                    tempSelectedMapObjects.Add(newObj);
                }
            }
            
        }
        private void CutSensors()
        {
            var selectedElements = GetAllMapElements().Where(x => IsElementSelected(x)).ToList();
            _clipboard.Copy(selectedElements);
        }


        #endregion
        #region CanvasEvents
        private void _canvas_Drop(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);
            if (data is MapObject mapData && IsEditMode)
            {  
                if (mapData != null)
                {
                    Point dropPosition = e.GetPosition(_canvas);
                    FrameworkElement element = CreateMapObject(mapData, _transformObject.WorldToScreen(dropPosition, _viewMatrix));
                    _isDropAdd = true;
                    var command = new AddSensor(mapData, (FrameworkElement)element, _canvas!, ItemsSource);
                    AddSensorsCommand.Execute(command);
                    if (mapData.Id == 0)
                    {
                        var existingIds = GetAllMapElements().Select(e2 => e2.MapData.Id).ToList();
                        mapData.Id = existingIds.Any() ? existingIds.Max() + 1 : 1;
                    }
                    if (element is FrameworkElement feDrop) feDrop.Tag = mapData.Id;
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
            foreach (FrameworkElement element in _canvas!.Children)
            {
                if (element != null)
                {
                    if (element is IMapElement mapElement)
                    {
                        var data = ItemsSource.Where(x => x == mapElement.MapData).FirstOrDefault();
                        if (data == null) continue;
                        Point screen = _transformObject.WorldToScreen(new Point(Canvas.GetLeft(element), Canvas.GetTop(element)), _viewMatrix);
                        Canvas.SetLeft(element, screen.X);
                        Canvas.SetTop(element, screen.Y);
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
       
        private FrameworkElement CreateMapObject(MapObject mapData, Point point)
        {
            if (mapData is SensorAssignments sensorData)
            {
                var element = new CustomSensor();
                element.SensorData = sensorData;
                element.CustomBackground = (SolidColorBrush)(new BrushConverter().ConvertFrom(sensorData.Sensor?.SensorType?.Color ?? Colors.PaleVioletRed.ToString()));
                element.Focus();
                Canvas.SetLeft(element, point.X);
                Canvas.SetTop(element, point.Y);
                element.SensorData.X = point.X;
                element.SensorData.Y = point.Y;
                return element;
            }
            else if (mapData is DeviceAssignment deviceData)
            {
                var element = new CustomDevice();
                element.DeviceData = deviceData;
                element.CustomBackground = new SolidColorBrush(Colors.LightBlue);
                element.Focus();
                Canvas.SetLeft(element, point.X);
                Canvas.SetTop(element, point.Y);
                element.DeviceData.X = point.X;
                element.DeviceData.Y = point.Y;
                return element;
            }
            return null!;
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
        private List<MapObject> GetMapObjectsInSelectionRectangle(Point pos)
        {
            if (SelectionRect.Width > 5 && SelectionRect.Height > 5)
            {
                double left = Math.Min(_initialMousePosition.X, pos.X);
                double right = Math.Max(_initialMousePosition.X, pos.X);
                double top = Math.Min(_initialMousePosition.Y, pos.Y);
                double bottom = Math.Max(_initialMousePosition.Y, pos.Y);

                var collectionMapObjects = (from mapObj in ItemsSource
                                        where !(mapObj.X + mapObj.Width < left ||
                                              mapObj.X > right ||
                                              mapObj.Y + mapObj.Height < top ||
                                              mapObj.Y > bottom)
                                        select mapObj).ToList();
                foreach (var mapObj in collectionMapObjects)
                {
                    if (_canvas != null)
                    {
                        var elementToSelect = FindMapElement(_canvas, mapObj);
                        if (elementToSelect is CustomSensor cs)
                        {
                            cs.SetCurrentValue(CustomSensor.IsSelectedProperty, true);
                            cs.IsSelected = true;
                            Canvas.SetZIndex(cs, 1);
                        }
                        else if (elementToSelect is CustomDevice cd)
                        {
                            cd.SetCurrentValue(CustomDevice.IsSelectedProperty, true);
                            cd.IsSelected = true;
                            Canvas.SetZIndex(cd, 1);
                        }
                    }
                }
                foreach (var mapObj in tempSelectedMapObjects.Except(collectionMapObjects))
                {
                    if (_canvas != null)
                    {
                        var elementToUnSelect = FindMapElement(_canvas, mapObj);
                        if (elementToUnSelect is CustomSensor cs)
                        {
                            cs.SetCurrentValue(CustomSensor.IsSelectedProperty, false);
                            cs.IsSelected = false;
                            Canvas.SetZIndex(cs, 0);
                        }
                        else if (elementToUnSelect is CustomDevice cd)
                        {
                            cd.SetCurrentValue(CustomDevice.IsSelectedProperty, false);
                            cd.IsSelected = false;
                            Canvas.SetZIndex(cd, 0);
                        }
                    }
                }

                return new List<MapObject>(collectionMapObjects);
            }
            return new List<MapObject>();
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
            if (constrainedRect.IsEmpty) return;
            SelectionRect.Width = constrainedRect.Width;
            SelectionRect.Height = constrainedRect.Height;
            Canvas.SetLeft(SelectionRect, constrainedRect.Left);
            Canvas.SetTop(SelectionRect, constrainedRect.Top);
            
            tempSelectedMapObjects = GetMapObjectsInSelectionRectangle(pos);
            IsMultiSelection = tempSelectedMapObjects.Count > 1;
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
        public void ClearSelectedSensors()
        {
            var selectedElements = GetAllMapUIElements()
                    .Where(e => e is IMapElement me && IsElementSelected(me))
                    .ToList();
            foreach (var uiElement in selectedElements)
            {
                if (uiElement is CustomSensor cs)
                {
                    cs.SetCurrentValue(CustomSensor.IsSelectedProperty, false);
                    cs.IsSelected = false;
                }
                else if (uiElement is CustomDevice cd)
                {
                    cd.SetCurrentValue(CustomDevice.IsSelectedProperty, false);
                    cd.IsSelected = false;
                }
                Canvas.SetZIndex(uiElement, 0);
            }
            tempSelectedMapObjects.Clear();
            IsMultiSelection = false;
        }
    }
}

