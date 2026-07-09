using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using SensorMap.Commands.SensorCommands;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Control = System.Windows.Controls.Control;
using Image = System.Windows.Controls.Image;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;

namespace SensorMap.CustomControls
{
    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_Device", Type = typeof(Border))]
    [TemplatePart(Name = "PART_Address", Type = typeof(TextBlock))]
    public class CustomDevice : Control, ICloneable, IMapElement
    {
        #region Dependency Properties

        public DeviceAssignment DeviceData
        {
            get { return (DeviceAssignment)GetValue(DeviceProperty); }
            set { SetValue(DeviceProperty, value); }
        }
        public static readonly DependencyProperty DeviceProperty =
            DependencyProperty.Register("Device", typeof(DeviceAssignment), typeof(CustomDevice), new PropertyMetadata(null, DeviceDataChanged));

        MapObject IMapElement.MapData => DeviceData;
        public void SetCustomBounds(Rect bounds) => CustomBounds = bounds;
        public Rect GetCustomBounds() => CustomBounds;

        private static void DeviceDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CustomDevice)d;
            if (e.NewValue != null && e.NewValue is DeviceAssignment value)
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
                typeof(CustomDevice),
                new PropertyMetadata(new SolidColorBrush(Colors.LightBlue)));

        public SolidColorBrush CustBorderBrush
        {
            get { return (SolidColorBrush)GetValue(CustBorderBrushProperty); }
            set { SetValue(CustBorderBrushProperty, value); }
        }
        public static readonly DependencyProperty CustBorderBrushProperty =
            DependencyProperty.Register("CustBorderBrush", typeof(SolidColorBrush),
                typeof(CustomDevice),
                new PropertyMetadata(Brushes.Black));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); SelectedChanged(); }
        }
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(CustomDevice), new PropertyMetadata(false));

        public bool IsEditMode
        {
            get { return (bool)GetValue(IsEditModeProperty); }
            set { SetValue(IsEditModeProperty, value); }
        }
        public static readonly DependencyProperty IsEditModeProperty =
            DependencyProperty.Register("IsEditMode", typeof(bool), typeof(CustomDevice),
                new PropertyMetadata(false, OnIsEditPropertyChanged));

        public bool IsMultiSelection
        {
            get { return (bool)GetValue(IsMultiSelectionProperty); }
            set { SetValue(IsMultiSelectionProperty, value); }
        }
        public static readonly DependencyProperty IsMultiSelectionProperty =
            DependencyProperty.Register("IsMultiSelection", typeof(bool),
                typeof(CustomDevice), new PropertyMetadata(false));

        private static void OnIsEditPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CustomDevice)d;
            if (e.NewValue != null && e.NewValue is bool value)
            {
                if (control._canvas != null) control.ChangeStateActions();
            }
        }

        public Rect CustomBounds
        {
            get => (Rect)GetValue(BoundsProperty);
            set => SetValue(BoundsProperty, value);
        }
        public static readonly DependencyProperty BoundsProperty = DependencyProperty.Register("CustomBounds", typeof(Rect),
            typeof(CustomDevice), new FrameworkPropertyMetadata(
                new Rect(0, 0, 50, 30),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnCustomBoundsChanged));

        private static void OnCustomBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var device = (CustomDevice)d;
            var newRect = (Rect)e.NewValue;
            device.CustomBounds = newRect;
        }

        public ICommand TransformCommand
        {
            get { return (ICommand)GetValue(TransformCommandProperty); }
            set { SetValue(TransformCommandProperty, value); }
        }
        public static readonly DependencyProperty TransformCommandProperty =
            DependencyProperty.Register("TransformCommand", typeof(ICommand),
                typeof(CustomDevice),
                new PropertyMetadata(null));

        public CustomDevice SelectedDevice
        {
            get { return (CustomDevice)GetValue(SelectedDeviceProperty); }
            set { SetValue(SelectedDeviceProperty, value); }
        }
        public static readonly DependencyProperty SelectedDeviceProperty =
            DependencyProperty.Register("SelectedDevice", typeof(CustomDevice),
                typeof(CustomDevice),
                new PropertyMetadata(null));

        public bool IsDragging
        {
            get { return (bool)GetValue(IsDraggingProperty); }
            set { SetValue(IsDraggingProperty, value); }
        }
        public static readonly DependencyProperty IsDraggingProperty =
            DependencyProperty.Register("IsDragging", typeof(bool), typeof(CustomDevice), new PropertyMetadata(false));

        public HitType MouseHitType
        {
            get { return (HitType)GetValue(MouseHitTypeProperty); }
            set { SetValue(MouseHitTypeProperty, value); }
        }
        public static readonly DependencyProperty MouseHitTypeProperty =
            DependencyProperty.Register("MouseHitType", typeof(HitType), typeof(CustomDevice), new PropertyMetadata(HitType.None));

        public bool IsSelectionRectActive
        {
            get { return (bool)GetValue(IsSelectionRectActiveProperty); }
            set { SetValue(IsSelectionRectActiveProperty, value); }
        }
        public static readonly DependencyProperty IsSelectionRectActiveProperty =
            DependencyProperty.Register("IsSelectionRectActive", typeof(bool), typeof(CustomDevice), new PropertyMetadata(false));

        private static CustomDevice _memorySelectedDevice;

        #endregion

        private enum AddressPlacement { Right, Left, Bottom, Top, Center }

        private readonly ITransformObject _transformService;
        private Point LastPoint;
        private bool IsTransformed = false;
        private Canvas _canvas;
        private TextBlock _textBlock;
        Rect addressRect;
        Rect Map;
        private Image _image;
        private bool IsMoving;
        private AddressPlacement _addressPlacement = AddressPlacement.Right;

        static CustomDevice()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomDevice), new FrameworkPropertyMetadata(typeof(CustomDevice)));
        }

        public CustomDevice()
        {
            _transformService = new TransformObjectService();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _canvas = _transformService.GetParentCanvas(this);
            _image = _canvas.Children.OfType<Image>().First();
            _textBlock = (TextBlock)GetTemplateChild("PART_Address");
            if (_canvas != null)
            {
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
                this.MouseMove += OnDeviceMouseMove;
                _canvas.MouseMove += OnMouseMove;
                _canvas.MouseUp += OnMouseUp;
                this.MouseLeave += OnMouseLeave;
            }
            else
            {
                this.IsSelected = false;
                this.MouseDown -= OnMouseDown;
                this.MouseMove -= OnDeviceMouseMove;
                _canvas.MouseMove -= OnMouseMove;
                _canvas.MouseUp -= OnMouseUp;
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsDragging)
            {
                MouseHitType = HitType.None;
                Cursor = _transformService.GetCursorForHitType(HitType.None);
            }
        }

        private void OnDeviceMouseMove(object sender, MouseEventArgs e)
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
            var selectedDevices = _canvas.Children.OfType<CustomDevice>()
                   .Where(x => x.IsSelected).Select(x => x.DeviceData).Cast<MapObject>().ToList();
            var canvasCollection = _canvas.Children.OfType<CustomDevice>()
                .Where(x => selectedDevices.Contains(x.DeviceData)).ToList().Cast<UIElement>().ToList();
            if (IsTransformed || IsMoving)
            {
                var command = new TransformationSensors(selectedDevices, canvasCollection, (x) => _transformService.WorldToScreen(worldPoint, MapProperties.GetViewMatrix(this)));
                TransformCommand.Execute(command);
            }
            e.Handled = true;
            IsDragging = false;
            IsMoving = false;
            IsTransformed = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (SelectedDevice != null)
            {
                if (IsDragging)
                {
                    Point point = Mouse.GetPosition(_canvas);
                    double offset_x = point.X - LastPoint.X;
                    double offset_y = point.Y - LastPoint.Y;

                    foreach (var item in _canvas.Children.OfType<CustomDevice>().Where(x => x.IsSelected))
                    {
                        double new_x = Canvas.GetLeft(item);
                        double new_y = Canvas.GetTop(item);
                        double new_width = item.CustomBounds.Width;
                        double new_height = item.CustomBounds.Height;

                        if (MouseHitType == HitType.Body && MouseHitType != HitType.None)
                        {
                            new_x += offset_x;
                            new_y += offset_y;
                            IsMoving = true;
                        }
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

                                item.CustomBounds = new Rect(new_x, new_y, new_width, new_height);
                                ChangeAddressPosition();
                                LastPoint = point;
                            }
                        }
                    }
                }
            }
        }

        private void ChangeAddressPosition()
        {
            if (!IsTransformed && !IsMoving) return;
            ResolveAddressPlacement();
            ApplyAddressPlacement();
        }

        public void UpdateAddressPosition()
        {
            ResolveAddressPlacement();
            ApplyAddressPlacement();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_memorySelectedDevice != null && _memorySelectedDevice != this && !IsMultiSelection)
            {
                _memorySelectedDevice.IsSelected = false;
                _memorySelectedDevice = null;
                if (SelectedDevice != null)
                {
                    SelectedDevice.IsSelected = false;
                    SelectedDevice.CustBorderBrush = Brushes.Black;
                }
            }
            if (MouseHitType != HitType.None)
            {
                LastPoint = Mouse.GetPosition(_canvas);
                IsDragging = true;
            }

            SelectedDevice = this;
            _memorySelectedDevice = this;
            IsSelected = true;
            this.Focus();

            if (_image.Source != null)
                Map = new Rect(Canvas.GetLeft(_image), Canvas.GetTop(_image), _image.ActualWidth, _image.ActualHeight);
        }

        private void SelectedChanged()
        {
            SelectedDevice = this.IsSelected ? this : null;
            if (this.IsSelected) Canvas.SetZIndex(this, 1); else Canvas.SetZIndex(this, 0);
            CustBorderBrush = IsSelected ? Brushes.DarkGreen : Brushes.Black;
            _textBlock.Background = IsSelected ? Brushes.Lavender : Brushes.WhiteSmoke;
            Mouse.OverrideCursor = IsSelected ? _transformService.GetCursorForHitType(MouseHitType) : null;
        }

        public object Clone()
        {
            return new CustomDevice
            {
                DeviceData = DeviceData,
                CustomBounds = CustomBounds
            };
        }

        private void ResolveAddressPlacement()
        {
            if (_textBlock == null || _canvas == null) return;

            double deviceCanvasX = Canvas.GetLeft(this);
            double deviceCanvasY = Canvas.GetTop(this);
            double addrW = _textBlock.ActualWidth > 0 ? _textBlock.ActualWidth : 40;
            double addrH = _textBlock.ActualHeight > 0 ? _textBlock.ActualHeight : 15;

            var candidates = new (AddressPlacement placement, Rect addrRect)[]
            {
                (AddressPlacement.Right, new Rect(
                    deviceCanvasX + CustomBounds.Width + 5,
                    deviceCanvasY + (CustomBounds.Height - addrH) / 2,
                    addrW, addrH)),
                (AddressPlacement.Left, new Rect(
                    deviceCanvasX - addrW - 5,
                    deviceCanvasY + (CustomBounds.Height - addrH) / 2,
                    addrW, addrH)),
                (AddressPlacement.Bottom, new Rect(
                    deviceCanvasX + (CustomBounds.Width - addrW) / 2,
                    deviceCanvasY + CustomBounds.Height + 2,
                    addrW, addrH)),
                (AddressPlacement.Top, new Rect(
                    deviceCanvasX + (CustomBounds.Width - addrW) / 2,
                    deviceCanvasY - addrH - 2,
                    addrW, addrH)),
                (AddressPlacement.Center, new Rect(
                    deviceCanvasX + (CustomBounds.Width - addrW) / 2,
                    deviceCanvasY + (CustomBounds.Height - addrH) / 2,
                    addrW, addrH))
            };

            Rect searchArea = Rect.Union(addressRect, CustomBounds);
            searchArea.Inflate(20, 20);
            addressRect = candidates.Where(pos => pos.placement == _addressPlacement).Select(pos => pos.addrRect).First();
            var devicesInSearchArea = _canvas.Children.OfType<CustomDevice>()
                .Where(s => s != this)
                .Select(s =>
                {
                    var rectToCheck = s._textBlock.Visibility != Visibility.Collapsed
                        ? Rect.Union(s.GetAddressRectOnCanvas(), s.CustomBounds)
                        : s.CustomBounds;
                    return new { Device = s, CheckRect = rectToCheck };
                })
                .Where(x => x.CheckRect.IntersectsWith(searchArea))
                .Select(x => x.CheckRect).ToList();

            bool HasCollision(Rect addrRect)
            {
                foreach (var nearDevice in devicesInSearchArea)
                {
                    if (addrRect.IntersectsWith(nearDevice))
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
                if (!HasCollision(current.addrRect))
                    return;
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

        private void ApplyAddressPlacement()
        {
            if (_textBlock == null) return;

            double addrW = _textBlock.ActualWidth > 0 ? _textBlock.ActualWidth : 40;
            double addrH = _textBlock.ActualHeight > 0 ? _textBlock.ActualHeight : 15;
            double deviceW = CustomBounds.Width;
            double deviceH = CustomBounds.Height;

            switch (_addressPlacement)
            {
                case AddressPlacement.Right:
                    Canvas.SetLeft(_textBlock, deviceW + 5);
                    Canvas.SetTop(_textBlock, (deviceH - addrH) / 2);
                    _textBlock.Opacity = 1;
                    break;
                case AddressPlacement.Left:
                    Canvas.SetLeft(_textBlock, -addrW - 5);
                    Canvas.SetTop(_textBlock, (deviceH - addrH) / 2);
                    _textBlock.Opacity = 1;
                    break;
                case AddressPlacement.Bottom:
                    Canvas.SetLeft(_textBlock, (deviceW - addrW) / 2);
                    Canvas.SetTop(_textBlock, deviceH + 2);
                    _textBlock.Opacity = 1;
                    break;
                case AddressPlacement.Top:
                    Canvas.SetLeft(_textBlock, (deviceW - addrW) / 2);
                    Canvas.SetTop(_textBlock, -addrH - 2);
                    _textBlock.Opacity = 1;
                    break;
                case AddressPlacement.Center:
                    Canvas.SetLeft(_textBlock, (deviceW - addrW) / 2);
                    Canvas.SetTop(_textBlock, (deviceH - addrH) / 2);
                    _textBlock.Opacity = 0.7;
                    break;
            }
            addressRect = new Rect(Canvas.GetLeft(_textBlock) + CustomBounds.X, Canvas.GetTop(_textBlock) + CustomBounds.Y, _textBlock.Width, _textBlock.Height);
            ControlOutOfRangeImage();
        }

        private Rect GetAddressRectOnCanvas()
        {
            if (_textBlock == null) return Rect.Empty;

            double deviceCanvasX = Canvas.GetLeft(this);
            double deviceCanvasY = Canvas.GetTop(this);
            double addrLocalX = Canvas.GetLeft(_textBlock);
            double addrLocalY = Canvas.GetTop(_textBlock);

            return new Rect(
                deviceCanvasX + addrLocalX,
                deviceCanvasY + addrLocalY,
                _textBlock.ActualWidth > 0 ? _textBlock.ActualWidth : 40,
                _textBlock.ActualHeight > 0 ? _textBlock.ActualHeight : 15
            );
        }

        private void ControlOutOfRangeImage()
        {
            if (_textBlock == null) return;
            if (Map.Width == 0) return;

            double deviceCanvasX = Canvas.GetLeft(this);
            double deviceCanvasY = Canvas.GetTop(this);
            double addrLocalX = Canvas.GetLeft(_textBlock);
            double addrLocalY = Canvas.GetTop(_textBlock);
            double addrW = _textBlock.ActualWidth > 0 ? _textBlock.ActualWidth : 40;
            double addrH = _textBlock.ActualHeight > 0 ? _textBlock.ActualHeight : 15;

            double addrAbsX = deviceCanvasX + addrLocalX;
            double addrAbsY = deviceCanvasY + addrLocalY;

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
                double deviceW = CustomBounds.Width;
                double deviceH = CustomBounds.Height;
                switch (_addressPlacement)
                {
                    case AddressPlacement.Right:
                        Canvas.SetLeft(_textBlock, deviceW + 5);
                        Canvas.SetTop(_textBlock, (deviceH - addrH) / 2);
                        break;
                    case AddressPlacement.Left:
                        Canvas.SetLeft(_textBlock, -addrW - 5);
                        Canvas.SetTop(_textBlock, (deviceH - addrH) / 2);
                        break;
                    case AddressPlacement.Bottom:
                        Canvas.SetLeft(_textBlock, (deviceW - addrW) / 2);
                        Canvas.SetTop(_textBlock, deviceH + 2);
                        break;
                    case AddressPlacement.Top:
                        Canvas.SetLeft(_textBlock, (deviceW - addrW) / 2);
                        Canvas.SetTop(_textBlock, -addrH - 2);
                        break;
                }
            }
        }
    }
}
