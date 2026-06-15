using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using ReactiveUI;
using SensorMap.Commands.SensorCommands;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using SensorMap.ViewModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using static SensorMap.Services.TransformObjectService;
using Brushes = System.Windows.Media.Brushes;
using Control = System.Windows.Controls.Control;
using Image = System.Windows.Controls.Image;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;

namespace SensorMap.CustomControls
{
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_Sensor", Type = typeof(Border))]
    [TemplatePart(Name = "PART_Address", Type = typeof(TextBlock))]
    public class CustomSensor : Control, ICloneable
    {
        #region Dependency Properties


        public SensorAssignments SensorData
        {
            get { return (SensorAssignments)GetValue(SensorProperty); }
            set { SetValue(SensorProperty, value);}
        }
        public static readonly DependencyProperty SensorProperty =
            DependencyProperty.Register("Sensor", typeof(SensorAssignments), typeof(CustomSensor),new PropertyMetadata(null,SensorDataChanged));

        private static void SensorDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CustomSensor)d;
            if (e.NewValue != null && e.NewValue is SensorAssignments value)
            {
                Rect rect = new Rect(value.X, value.Y, value.Width, value.Height);
                control.CustomBounds = rect;
            }
        }
        public SolidColorBrush CustomBackground
        {
            get { return (SolidColorBrush)GetValue(CustomBackgroundProperty); }
            set { SetValue(CustomBackgroundProperty, value); }
        }

        public static readonly DependencyProperty CustomBackgroundProperty =
            DependencyProperty.Register(
                "CustomBackground", 
                typeof(SolidColorBrush), 
                typeof(CustomSensor), 
                new PropertyMetadata(new SolidColorBrush(Colors.WhiteSmoke)));
        public SolidColorBrush CustBorderBrush
        {
            get { return (SolidColorBrush)GetValue(CustBorderBrushProperty); }
            set { SetValue(CustBorderBrushProperty, value); }
        }

        public static readonly DependencyProperty CustBorderBrushProperty = DependencyProperty.Register("CustBorderBrush", typeof(SolidColorBrush), 
            typeof(CustomSensor), 
            new PropertyMetadata(Brushes.Black));
       
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); SelectedChanged(); }
        }
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool),
            typeof(CustomSensor), new PropertyMetadata(false));
        public bool IsEditMode
        {
            get { return (bool)GetValue(IsEditModeProperty); }
            set { SetValue(IsEditModeProperty, value); }
        }
        public static readonly DependencyProperty IsEditModeProperty =
            DependencyProperty.Register("IsEditMode", typeof(bool), typeof(CustomSensor),
                new PropertyMetadata(false,OnIsEditPropertyChanged));


        public bool IsMultiSelection
        {
            get { return (bool)GetValue(IsMultiSelectionProperty); }
            set { SetValue(IsMultiSelectionProperty, value); }
        }

        public static readonly DependencyProperty IsMultiSelectionProperty =
            DependencyProperty.Register("IsMultiSelection", typeof(bool),
                typeof(CustomSensor), new PropertyMetadata(false));
        

        private static void OnIsEditPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CustomSensor)d;
            if (e.NewValue != null && e.NewValue is bool value)
            {               
                if(control._canvas!=null) control.ChangeStateActions();
            }
        }
        public Rect CustomBounds
        {
            get => (Rect)GetValue(BoundsProperty);
            set => SetValue(BoundsProperty, value);
        }
        public static readonly DependencyProperty BoundsProperty = DependencyProperty.Register("CustomBounds", typeof(Rect),
            typeof(CustomSensor), new FrameworkPropertyMetadata(
                new Rect(0, 0, 30, 30),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnCustomBoundsChanged));

        private static void OnCustomBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sensor = (CustomSensor)d;
            var newRect = (Rect)e.NewValue;
            sensor.CustomBounds = newRect;
        }
        
        public ICommand TransformCommand
        {
            get { return (ICommand)GetValue(TransformCommandProperty); }
            set { SetValue(TransformCommandProperty, value); }
        }
        public static readonly DependencyProperty TransformCommandProperty =
            DependencyProperty.Register("TransformCommand", typeof(ICommand),
                typeof(CustomSensor),
                new PropertyMetadata(null));
       

        public CustomSensor SelectedSensor
        {
            get { return (CustomSensor)GetValue(SelectedSensorProperty); }
            set { SetValue(SelectedSensorProperty, value); }
        }
        public static readonly DependencyProperty SelectedSensorProperty =
            DependencyProperty.Register("SelectedSensor", typeof(CustomSensor),
                typeof(CustomSensor),
                new PropertyMetadata(null));

        public bool IsDragging
        {
            get { return (bool)GetValue(IsDraggingProperty); }
            set { SetValue(IsDraggingProperty, value); }
        }
        public static readonly DependencyProperty IsDraggingProperty =
            DependencyProperty.Register("IsDragging", typeof(bool), typeof(CustomSensor), new PropertyMetadata(false));



        public AddressPosition addressPosition
        {
            get { return (AddressPosition)GetValue(addressPositionProperty); }
            set { SetValue(addressPositionProperty, value); }
        }

        public static readonly DependencyProperty addressPositionProperty =
            DependencyProperty.Register("addressPosition", typeof(AddressPosition), typeof(CustomSensor), new PropertyMetadata(AddressPosition.Right));




        public HitType MouseHitType
        {
            get { return (HitType)GetValue(MouseHitTypeProperty); }
            set { SetValue(MouseHitTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseHitType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseHitTypeProperty =
            DependencyProperty.Register("MouseHitType", typeof(HitType), typeof(CustomSensor), new PropertyMetadata(HitType.None));


        public bool IsSelectionRectActive
        {
            get { return (bool)GetValue(IsSelectionRectActiveProperty); }
            set { SetValue(IsSelectionRectActiveProperty, value); }
        }
        public static readonly DependencyProperty IsSelectionRectActiveProperty =
            DependencyProperty.Register("IsSelectionRectActive", typeof(bool), typeof(CustomSensor), new PropertyMetadata(false));

        private static CustomSensor _memorySelectedSensor;

        #endregion

        private enum AddressPlacement { Right, Left, Bottom, Top, Center }

        private readonly ITransformObject _transformService;
        private Point LastPoint;
        private bool IsTransformed = false;
        private  Canvas _canvas;
        private TextBlock _textBlock;
        private System.Windows.Controls.Image _image;
        private bool IsMoving;
        private bool AddressLeft = false;
        private bool AddressBottom = false;

        static CustomSensor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomSensor), new FrameworkPropertyMetadata(typeof(CustomSensor)));
        }
        public CustomSensor()
        {
            _transformService = new TransformObjectService();
        }
        
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _canvas = _transformService.GetParentCanvas(this);
            _image = _canvas.Children.OfType<Image>().First();
            _textBlock = (TextBlock)GetTemplateChild("PART_Address");
            addressRect = new Rect(Canvas.GetLeft(_textBlock) + CustomBounds.X, Canvas.GetTop(_textBlock) + CustomBounds.Y, _textBlock.Width, _textBlock.Height);
            if ( _canvas != null )
                ChangeStateActions();
                System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Loaded,
                    new Action(() => UpdateAddressPosition()));
                
            }
        }
        
        private void ChangeStateActions()
        {
            
            if (IsEditMode)
            {
                this.MouseDown += OnMouseDown;
                this.MouseMove += OnSensorMouseMove;
                _canvas.MouseMove += OnMouseMove;
                _canvas.MouseUp += OnMouseUp;
                this.MouseLeave += OnMouseLeave;
            }
            else
            {
                this.IsSelected = false;
                this.MouseDown -= OnMouseDown;
                this.MouseMove -= OnSensorMouseMove;
                _canvas.MouseMove -= OnMouseMove;
                _canvas.MouseUp -= OnMouseUp;
            }
        }

        private void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsDragging)
            {
                MouseHitType = HitType.None;
                Cursor = _transformService.GetCursorForHitType(HitType.None);
            }
        }
        private void OnSensorMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.IsSelected && !IsDragging && !IsSelectionRectActive)
            {
                if (IsMultiSelection) MouseHitType = HitType.Body;
                else
                {
                    Rect rect = new Rect(Canvas.GetLeft(this), Canvas.GetTop(this), this.CustomBounds.Width, this.CustomBounds.Height);
                    MouseHitType = _transformService.GetHitType(rect, Mouse.GetPosition(_canvas));
                }
                this.Cursor = _transformService.GetCursorForHitType(MouseHitType);
            }            
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsDragging == false) return;
                double screenX = Canvas.GetLeft(this);
                double screenY = Canvas.GetTop(this);

            Point worldPoint = _transformService.ScreenToWorld(new Point(screenX, screenY), MapProperties.GetViewMatrix(this));
            List<SensorAssignments> SelectedSensors = _canvas.Children.OfType<CustomSensor>()
                   .Where(x => x.IsSelected).Select(x => x.SensorData).ToList();
            var canvasCollection = _canvas.Children.OfType<CustomSensor>().Where(x => SelectedSensors.Contains(x.SensorData)).ToList();
            if (IsTransformed || IsMoving)
            {
                var command = new TransformationSensors(SelectedSensors, canvasCollection, (x) => _transformService.WorldToScreen(worldPoint, MapProperties.GetViewMatrix(this)));
                TransformCommand.Execute(command);
            }
            e.Handled = true;
            IsDragging = false;
            IsMoving = false;
            IsTransformed = false;
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (SelectedSensor != null)
            {
                if (IsDragging)
                {
                    Point point = Mouse.GetPosition(_canvas);
                    double offset_x = point.X - LastPoint.X;
                    MouseMoveUp = offsetVector.Y > 0 ? true : false;
                    foreach (var item in _canvas.Children.OfType<CustomSensor>().Where(x => x.IsSelected))
                    {
                        double new_x = Canvas.GetLeft(item);
                        double new_y = Canvas.GetTop(item);
                        double new_width = item.CustomBounds.Width;
                        double new_height = item.CustomBounds.Height;

                        // Перемещение
                        if (MouseHitType == HitType.Body && MouseHitType != HitType.None)
                        {
                            new_x += offset_x;
                            new_y += offset_y;
                            IsMoving = true;
                        }
                        //Трансформация
                        else if (!IsMultiSelection)
                        {
                            switch (MouseHitType)
                            {
                                case HitType.UpLeft:
                                    new_x += offset_x;
                                    new_y += offset_y;
                                    new_width -= offset_x;
                                    new_height -= offset_y;
                                    IsTransformed = true;
                                    break;
                                case HitType.UpRight:
                                    new_y += offset_y;
                                    new_width += offset_x;
                                    new_height -= offset_y;
                                    IsTransformed = true;
                                    break;
                                case HitType.BottomRight:
                                    new_width += offset_x;
                                    new_height += offset_y;
                                    IsTransformed = true;
                                    break;
                                case HitType.BottomLeft:
                                    new_x += offset_x;
                                    new_width -= offset_x;
                                    new_height += offset_y;
                                    IsTransformed = true;
                                    break;
                                case HitType.Left:
                                    new_x += offset_x;
                                    new_width -= offset_x;
                                    IsTransformed = true;
                                    break;
                                case HitType.Right:
                                    new_width += offset_x;
                                    IsTransformed = true;
                                    break;
                                case HitType.Bottom:
                                    new_height += offset_y;
                                    IsTransformed = true;
                                    break;
                                case HitType.Top:
                                    new_y += offset_y;
                                    new_height -= offset_y;
                                    IsTransformed = true;
                                    break;
                            }
                        }
                        if ((new_x > 1 && new_x < _image.ActualWidth - new_width) && (new_y > 1 && new_y < _image.ActualHeight - new_height))
                        {
                            if ((new_width > 17) && (new_height > 17))
                            {
                                Canvas.SetLeft(item, new_x);
                                Canvas.SetTop(item, new_y);
                                
                                var oldBounds = item.CustomBounds;
                                item.CustomBounds = new Rect(new_x, new_y, new_width, new_height);
                                UpdateAddressRect();
                                _transformService.NoCollisionWithRect(_textBlock,addressRect,new Rect(_image.RenderSize), CustomBounds);
                                
                                ChangeAddressPosition();
                                LastPoint = point; 
                                
                            }
                        }
                    }
                }
            }
        }
        private void ChangeAddressPosition()
            if (!IsTransformed && !IsMoving) return;
            ResolveAddressPlacement();
            ApplyAddressPlacement();
        }
            int bottomSet = Convert.ToInt32(CustomBounds.Height);
        public void UpdateAddressPosition()
        {
            ResolveAddressPlacement();
            ApplyAddressPlacement();
        }
            else if (IsTransformed) Canvas.SetLeft(_textBlock, rightSet);
        private void ResolveAddressPlacement()
        {
            if (_textBlock == null || _canvas == null) return;
            else if (IsTransformed) Canvas.SetTop(_textBlock, topSet);
            double sensorCanvasX = Canvas.GetLeft(this);
            double sensorCanvasY = Canvas.GetTop(this);
            double addrW = _textBlock.ActualWidth > 0 ? _textBlock.ActualWidth : 40;
            double addrH = _textBlock.ActualHeight > 0 ? _textBlock.ActualHeight : 15;
            double verticalInflate = _textBlock.ActualHeight;
            var candidates = new (AddressPlacement placement, Rect addrRect)[]
            foreach (var neighbor in neighborsSensors)
                (AddressPlacement.Right, new Rect(
                    sensorCanvasX + CustomBounds.Width + 5,
                    sensorCanvasY + (CustomBounds.Height - addrH) / 2,
                    addrW, addrH)),
                (AddressPlacement.Left, new Rect(
                    sensorCanvasX - addrW - 5,
                    sensorCanvasY + (CustomBounds.Height - addrH) / 2,
                    addrW, addrH)),
                (AddressPlacement.Bottom, new Rect(
                    sensorCanvasX + (CustomBounds.Width - addrW) / 2,
                    sensorCanvasY + CustomBounds.Height + 2,
                    addrW, addrH)),
                (AddressPlacement.Top, new Rect(
                    sensorCanvasX + (CustomBounds.Width - addrW) / 2,
                    sensorCanvasY - addrH - 2,
                    addrW, addrH)),
            };
                int PointNeighborWithRightAddress = Convert.ToInt32(neighbor.CustomBounds.X);
            var others = _canvas.Children.OfType<CustomSensor>()
                .Where(s => s != this && s.SensorData.Id != SensorData.Id).ToList();
                //}
            bool HasCollision(Rect addrRect)
            {
                foreach (var other in others)
                {
                    double otherX = Canvas.GetLeft(other);
                    double otherY = Canvas.GetTop(other);
                    Rect otherBounds = new Rect(otherX, otherY, other.CustomBounds.Width, other.CustomBounds.Height);
                //        //необходимо переместить адрес направо
                    if (addrRect.IntersectsWith(otherBounds))
                        return true;

                    Rect otherAddrRect = other.GetAddressRectOnCanvas();
                    if (!otherAddrRect.IsEmpty && addrRect.IntersectsWith(otherAddrRect))
                        return true;
                }
                return false;
            }

            if (_addressPlacement == AddressPlacement.Center)
            {
                if (!HasCollision(candidates[0].addrRect))
                {
                    _addressPlacement = AddressPlacement.Right;
                    return;
                }
            }
            else
            {
                var current = candidates.FirstOrDefault(c => c.placement == _addressPlacement);
                if (current.placement != 0 || !current.addrRect.IsEmpty)
                {
                    if (!HasCollision(current.addrRect))
                        return;
                }
            }
                }
            foreach (var (placement, addrRect) in candidates)
            {
                if (!HasCollision(addrRect))
                {
                    _addressPlacement = placement;
                    return;
                }
            }

            _addressPlacement = AddressPlacement.Center;
        }
                }
        private void ApplyAddressPlacement()
        {
            if (_textBlock == null) return;

            double addrW = _textBlock.ActualWidth > 0 ? _textBlock.ActualWidth : 40;
            double addrH = _textBlock.ActualHeight > 0 ? _textBlock.ActualHeight : 15;
            double sensorW = CustomBounds.Width;
            double sensorH = CustomBounds.Height;

            switch (_addressPlacement)
            {
                case AddressPlacement.Right:
                    Canvas.SetLeft(_textBlock, sensorW + 5);
                    Canvas.SetTop(_textBlock, (sensorH - addrH) / 2);
                    break;
                case AddressPlacement.Left:
                    Canvas.SetLeft(_textBlock, -addrW - 5);
                    Canvas.SetTop(_textBlock, (sensorH - addrH) / 2);
                    break;
                case AddressPlacement.Bottom:
                    Canvas.SetLeft(_textBlock, (sensorW - addrW) / 2);
                    Canvas.SetTop(_textBlock, sensorH + 2);
                    break;
                case AddressPlacement.Top:
                    Canvas.SetLeft(_textBlock, (sensorW - addrW) / 2);
                    Canvas.SetTop(_textBlock, -addrH - 2);
                    break;
                case AddressPlacement.Center:
                    Canvas.SetLeft(_textBlock, (sensorW - addrW) / 2);
                    Canvas.SetTop(_textBlock, (sensorH - addrH) / 2);
                    break;
            }

            ControlOutOfRangeImage();
        }

        private Rect GetAddressRectOnCanvas()
        {
            if (_textBlock == null) return Rect.Empty;

            double sensorCanvasX = Canvas.GetLeft(this);
            double sensorCanvasY = Canvas.GetTop(this);
            double addrLocalX = Canvas.GetLeft(_textBlock);
            double addrLocalY = Canvas.GetTop(_textBlock);

            return new Rect(
                sensorCanvasX + addrLocalX,
                sensorCanvasY + addrLocalY,
                _textBlock.ActualWidth > 0 ? _textBlock.ActualWidth : 40,
                _textBlock.ActualHeight > 0 ? _textBlock.ActualHeight : 15
            );
        }
        private void ControlOutOfRangeImage()
        {
            if (_textBlock == null) return;

            double sensorCanvasX = Canvas.GetLeft(this);
            double sensorCanvasY = Canvas.GetTop(this);
            double addrLocalX = Canvas.GetLeft(_textBlock);
            double addrLocalY = Canvas.GetTop(_textBlock);
            double addrW = _textBlock.ActualWidth > 0 ? _textBlock.ActualWidth : 40;
            double addrH = _textBlock.ActualHeight > 0 ? _textBlock.ActualHeight : 15;
        }
            double addrAbsX = sensorCanvasX + addrLocalX;
            double addrAbsY = sensorCanvasY + addrLocalY;

            bool moved = false;
            if (addrAbsX < 2)
            {
                _addressPlacement = AddressPlacement.Right;
                moved = true;
            }
            else if (addrAbsX + addrW > Map.Width)
            {
                _addressPlacement = AddressPlacement.Left;
                moved = true;
            }

            if (addrAbsY < 2)
            {
                _addressPlacement = AddressPlacement.Bottom;
                moved = true;
            }
            else if (addrAbsY + addrH > Map.Height)
            {
                _addressPlacement = AddressPlacement.Top;
                moved = true;
            }

            if (moved)
            {
                double sensorW = CustomBounds.Width;
                double sensorH = CustomBounds.Height;
                switch (_addressPlacement)
                {
                    case AddressPlacement.Right:
                        Canvas.SetLeft(_textBlock, sensorW + 5);
                        Canvas.SetTop(_textBlock, (sensorH - addrH) / 2);
                        break;
                    case AddressPlacement.Left:
                        Canvas.SetLeft(_textBlock, -addrW - 5);
                        Canvas.SetTop(_textBlock, (sensorH - addrH) / 2);
                        break;
                    case AddressPlacement.Bottom:
                        Canvas.SetLeft(_textBlock, (sensorW - addrW) / 2);
                        Canvas.SetTop(_textBlock, sensorH + 2);
                        break;
                    case AddressPlacement.Top:
                        Canvas.SetLeft(_textBlock, (sensorW - addrW) / 2);
                        Canvas.SetTop(_textBlock, -addrH - 2);
                        break;
                }
                AddressBottom = true;
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_memorySelectedSensor != null && _memorySelectedSensor != this && !IsMultiSelection)
            {
                _memorySelectedSensor.IsSelected = false;
                _memorySelectedSensor = null;
                if (SelectedSensor != null)
                {
                    SelectedSensor.IsSelected = false;
                    SelectedSensor.CustBorderBrush = Brushes.Black;
                }
            }
            if (MouseHitType!=HitType.None)
            {
                LastPoint = Mouse.GetPosition(_canvas);
                IsDragging = true;
            }
            
            SelectedSensor = this;
            _memorySelectedSensor = this;
            IsSelected = true;
            this.Focus();
            ChangeAddressPosition();
            
        }
        private void UpdateAddressRect()
        {
            addressRect = new Rect(Canvas.GetLeft(_textBlock) + CustomBounds.X, Canvas.GetTop(_textBlock) + CustomBounds.Y, _textBlock.ActualWidth, _textBlock.ActualHeight);
            
        }
        
        
        private void SelectedChanged()
        {
            SelectedSensor = this.IsSelected ? this : null;
            if (this.IsSelected) Canvas.SetZIndex(this, 1); else Canvas.SetZIndex(this, 0);
            CustBorderBrush = IsSelected ? Brushes.DarkGreen : Brushes.Black;
            _textBlock.Background = IsSelected ? Brushes.Lavender : Brushes.WhiteSmoke;
            Mouse.OverrideCursor = IsSelected? _transformService.GetCursorForHitType(MouseHitType) : null;
        }

        public object Clone()
        {
            return new CustomSensor
            {
                SensorData = SensorData,
                CustomBounds = CustomBounds
            };
        }




        private Rect addressRect;
        
        
    }
}
