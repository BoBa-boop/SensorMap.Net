using HandyControl.Controls;
using HandyControl.Interactivity;
using HandyControl.Properties.Langs;
using HandyControl.Tools;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SensorMap.CustomControls
{
    [TemplatePart(Name = "PART_PanelMain", Type = typeof(Panel))]
    //[TemplatePart(Name = "PART_BorderMove", Type = typeof(Border))]
    [TemplatePart(Name = "PART_BorderBottom", Type = typeof(Border))]
    [TemplatePart(Name = "PART_ImageMain", Type = typeof(Image))]
    public class ImageViewerCC : Control
    {
        #region const
        private const string ElementPanelMain = "PART_PanelMain";

        private const string ElementCanvasSmallImg = "PART_CanvasSmallImg";

        private const string ElementBorderMove = "PART_BorderMove";

        private const string ElementBorderBottom = "PART_BorderBottom";

        private const string ElementImageMain = "PART_ImageMain";
        private const double ScaleInternal = 0.2;
        #endregion
        #region fields
        private static readonly SaveFileDialog SaveFileDialog = new()
        {
            Filter = $"{Lang.PngImg}|*.png"
        };
        private Panel? _panelMain;
        //private Border _borderMove;
        private Border? _borderBottom;

        private Image? _imageMain;
        private bool _canMoveX;
        private bool _canMoveY;
        private Thickness _imgActualMargin;
        private double _imgActualRotate;

        private double _imgActualScale = 1.0;

        private Point _imgCurrentPoint;

        private bool _imgIsMouseDown;

        private Thickness _imgMouseDownMargin;

        private Point _imgMouseDownPoint;

        private double _imgWidHeiScale;

        private bool _isOblique;

        private double _scaleInternalHeight;

        private double _scaleInternalWidth;

        private bool _showBorderBottom;

        private DispatcherTimer? _dispatcher;

        private bool _isLoaded;

        private MouseBinding? _mouseMoveBinding;
        #endregion
        #region dpProp

        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(BitmapFrame), typeof(ImageViewerCC),
            new PropertyMetadata(default(BitmapFrame), OnImageSourceChanged));

        public static readonly DependencyProperty MaxScaleProperty = DependencyProperty.Register("MaxScale", typeof(double), typeof(ImageViewerCC),
            new PropertyMetadata(20.0));

        public static readonly DependencyProperty MinScaleProperty = DependencyProperty.Register("MinScale", typeof(double), typeof(ImageViewerCC),
            new PropertyMetadata(0.2));

        public static readonly DependencyProperty ShowImgMapProperty = DependencyProperty.Register("ShowImgMap", typeof(bool), typeof(ImageViewerCC));

        //public static readonly DependencyProperty UriProperty = DependencyProperty.Register("Uri", typeof(Uri), typeof(ImageViewerCC), new PropertyMetadata(null, OnUriChanged));

        public static readonly DependencyProperty ShowToolBarProperty = DependencyProperty.Register("ShowToolBar", typeof(bool), typeof(ImageViewerCC));

        public static readonly DependencyProperty IsFullScreenProperty = DependencyProperty.Register("IsFullScreen", typeof(bool), typeof(ImageViewerCC));

        public static readonly DependencyProperty MoveGestureProperty = DependencyProperty.Register("MoveGesture", typeof(MouseGesture), typeof(ImageViewerCC), new UIPropertyMetadata(new MouseGesture(MouseAction.LeftClick), OnMoveGestureChanged));

        internal static readonly DependencyProperty ImgPathProperty = DependencyProperty.Register("ImgPath", typeof(string), typeof(ImageViewerCC), new PropertyMetadata((object)null));

        internal static readonly DependencyProperty ImgSizeProperty = DependencyProperty.Register("ImgSize", typeof(long), typeof(ImageViewerCC), new PropertyMetadata(-1L));

        internal static readonly DependencyProperty ShowFullScreenButtonProperty = DependencyProperty.Register("ShowFullScreenButton", typeof(bool), typeof(ImageViewerCC));

        internal static readonly DependencyProperty ShowCloseButtonProperty = DependencyProperty.Register("ShowCloseButton", typeof(bool), typeof(ImageViewerCC));

        internal static readonly DependencyProperty ImageContentProperty = DependencyProperty.Register("ImageContent", typeof(object), typeof(ImageViewerCC), new PropertyMetadata((object)null));

        internal static readonly DependencyProperty ImageMarginProperty = DependencyProperty.Register("ImageMargin", typeof(Thickness), typeof(ImageViewerCC), new PropertyMetadata(default(Thickness)));

        internal static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register("ImageWidth", typeof(double), typeof(ImageViewerCC));

        internal static readonly DependencyProperty ImageHeightProperty = DependencyProperty.Register("ImageHeight", typeof(double), typeof(ImageViewerCC));

        internal static readonly DependencyProperty ImageScaleProperty = DependencyProperty.Register("ImageScale", typeof(double), typeof(ImageViewerCC), new PropertyMetadata(OnImageScaleChanged));

        internal static readonly DependencyProperty ScaleStrProperty = DependencyProperty.Register("ScaleStr", typeof(string), typeof(ImageViewerCC), new PropertyMetadata("100%"));

        internal static readonly DependencyProperty ImageRotateProperty = DependencyProperty.Register("ImageRotate", typeof(double), typeof(ImageViewerCC));

        #endregion
        #region Prop
        public bool IsFullScreen
        {
            get
            {
                return (bool)GetValue(IsFullScreenProperty);
            }
            set
            {
                SetValue(IsFullScreenProperty, value);
            }
        }

        [ValueSerializer(typeof(MouseGestureValueSerializer))]
        [TypeConverter(typeof(MouseGestureConverter))]
        public MouseGesture MoveGesture
        {
            get
            {
                return (MouseGesture)GetValue(MoveGestureProperty);
            }
            set
            {
                SetValue(MoveGestureProperty, value);
            }
        }
        public bool ShowToolBar
        {
            get
            {
                return (bool)GetValue(ShowToolBarProperty);
            }
            set
            {
                SetValue(ShowToolBarProperty, value);
            }
        }
        public BitmapFrame ImageSource
        {
            get => (BitmapFrame)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }
        internal object ImageContent
        {
            get
            {
                return GetValue(ImageContentProperty);
            }
            set
            {
                SetValue(ImageContentProperty, value);
            }
        }

        internal string ImgPath
        {
            get
            {
                return (string)GetValue(ImgPathProperty);
            }
            set
            {
                SetValue(ImgPathProperty, value);
            }
        }

        internal long ImgSize
        {
            get
            {
                return (long)GetValue(ImgSizeProperty);
            }
            set
            {
                SetValue(ImgSizeProperty, value);
            }
        }

        internal Thickness ImageMargin
        {
            get
            {
                return (Thickness)GetValue(ImageMarginProperty);
            }
            set
            {
                SetValue(ImageMarginProperty, value);
            }
        }

        internal double ImageWidth
        {
            get
            {
                return (double)GetValue(ImageWidthProperty);
            }
            set
            {
                SetValue(ImageWidthProperty, value);
            }
        }

        internal double ImageHeight
        {
            get
            {
                return (double)GetValue(ImageHeightProperty);
            }
            set
            {
                SetValue(ImageHeightProperty, value);
            }
        }

        internal double ImageScale
        {
            get
            {
                return (double)GetValue(ImageScaleProperty);
            }
            set
            {
                SetValue(ImageScaleProperty, value);
            }
        }
        public double MaxScale
        {
            get
            {
                return (double)GetValue(MaxScaleProperty);
            }
            set
            {
                SetValue(MaxScaleProperty, value);
            }
        }
        public double MinScale
        {
            get
            {
                return (double)GetValue(MinScaleProperty);
            }
            set
            {
                SetValue(MinScaleProperty, value);
            }
        }
        internal string ScaleStr
        {
            get
            {
                return (string)GetValue(ScaleStrProperty);
            }
            set
            {
                SetValue(ScaleStrProperty, value);
            }
        }

        internal double ImageRotate
        {
            get
            {
                return (double)GetValue(ImageRotateProperty);
            }
            set
            {
                SetValue(ImageRotateProperty, value);
            }
        }

        private double ImageOriWidth { get; set; }
        private double ImageOriHeight { get; set; }
        internal bool ShowCloseButton
        {
            get
            {
                return (bool)GetValue(ShowCloseButtonProperty);
            }
            set
            {
                SetValue(ShowCloseButtonProperty, value);
            }
        }
        internal bool ShowBorderBottom
        {
            get
            {
                return _showBorderBottom;
            }
            set
            {
                if (_showBorderBottom != value)
                {
                    _borderBottom?.BeginAnimation(UIElement.OpacityProperty, value ? AnimationHelper.CreateAnimation(1.0, 100.0) : AnimationHelper.CreateAnimation(0.0, 400.0));
                    _showBorderBottom = value;
                }
            }
        }
        #endregion

        static ImageViewerCC()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageViewerCC), new FrameworkPropertyMetadata(typeof(ImageViewerCC)));
        }
        public ImageViewerCC()
        {
            
            base.CommandBindings.Add(new CommandBinding(ControlCommands.Save, ButtonSave_OnClick));
            //base.CommandBindings.Add(new CommandBinding(ControlCommands.Open, ButtonWindowsOpen_OnClick));
            base.CommandBindings.Add(new CommandBinding(ControlCommands.Restore, ButtonActual_OnClick));
            base.CommandBindings.Add(new CommandBinding(ControlCommands.Reduce, ButtonReduce_OnClick));
            base.CommandBindings.Add(new CommandBinding(ControlCommands.Enlarge, ButtonEnlarge_OnClick));
            base.CommandBindings.Add(new CommandBinding(ControlCommands.RotateLeft, ButtonRotateLeft_OnClick));
            base.CommandBindings.Add(new CommandBinding(ControlCommands.RotateRight, ButtonRotateRight_OnClick));
            base.CommandBindings.Add(new CommandBinding(ControlCommands.MouseMove, ImageMain_OnMouseDown));
            OnMoveGestureChanged(MoveGesture);
            base.Loaded += delegate
            {
                _isLoaded = true;
                Init();
            };
        }
        //public ImageViewerCC(Uri uri) : this() { Uri = uri; }
        //public ImageViewerCC(string path) : this(new Uri(path))  {  }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _panelMain = GetTemplateChild("PART_PanelMain") as Panel;
            //_borderMove = GetTemplateChild("PART_BorderMove") as Border;
            _imageMain = GetTemplateChild("PART_ImageMain") as Image;
            _borderBottom = GetTemplateChild("PART_BorderBottom") as Border;
            if (_imageMain != null)
            {
                RotateTransform rotateTransform = new RotateTransform();
                BindingOperations.SetBinding(rotateTransform, RotateTransform.AngleProperty, new Binding(ImageRotateProperty.Name)
                {
                    Source = this
                });
                _imageMain.LayoutTransform = rotateTransform;
            }
        }

        private static void OnImageScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ImageViewerCC imageViewer)
            {
                object newValue = e.NewValue;
                if (newValue is double)
                {
                    double num = (double)newValue;
                    imageViewer.ImageWidth = imageViewer.ImageOriWidth * num;
                    imageViewer.ImageHeight = imageViewer.ImageOriHeight * num;
                    imageViewer.ScaleStr = $"{num * 100.0:#0}%";
                }
            }
        }
        private void Init()
        {
            if (ImageSource == null || !_isLoaded)
            {
                return;
            }

            if (ImageSource.IsDownloading)
            {
                _dispatcher = new DispatcherTimer(DispatcherPriority.ApplicationIdle)
                {
                    Interval = TimeSpan.FromSeconds(1.0)
                };
                _dispatcher.Tick += Dispatcher_Tick;
                _dispatcher.Start();
                return;
            }

            double width;
            double height;
            if (!_isOblique)
            {
                width = ImageSource.PixelWidth;
                height = ImageSource.PixelHeight;
            }
            else
            {
                width = ImageSource.PixelHeight;
                height = ImageSource.PixelWidth;
            }

            ImageWidth = width;
            ImageHeight = height;
            ImageOriWidth = width;
            ImageOriHeight = height;
            _scaleInternalWidth = ImageOriWidth * 0.1;
            _scaleInternalHeight = ImageOriHeight * 0.1;
            if (Math.Abs(height - 0.0) < 0.001 || Math.Abs(width - 0.0) < 0.001)
            {
                HandyControl.Controls.MessageBox.Show(Lang.ErrorImgSize);
                return;
            }

            _imgWidHeiScale = width / height;
            double num3 = base.ActualWidth / base.ActualHeight;
            ImageScale = 1.0;
            if (_imgWidHeiScale > num3)
            {
                if (width > base.ActualWidth)
                {
                    ImageScale = base.ActualWidth / width;
                }
            }
            else if (height > base.ActualHeight)
            {
                ImageScale = base.ActualHeight / height;
            }

            ImageMargin = new Thickness((base.ActualWidth - ImageWidth) / 2.0, (base.ActualHeight - ImageHeight) / 2.0, 0.0, 0.0);
            _imgActualScale = ImageScale;
            _imgActualMargin = ImageMargin;
        }

        private void Dispatcher_Tick(object sender, EventArgs e)
        {
            if (_dispatcher != null)
            {
                if (ImageSource == null || !_isLoaded)
                {
                    _dispatcher.Stop();
                    _dispatcher.Tick -= Dispatcher_Tick;
                    _dispatcher = null;
                }
                else if (!ImageSource.IsDownloading)
                {
                    _dispatcher.Stop();
                    _dispatcher.Tick -= Dispatcher_Tick;
                    _dispatcher = null;
                    Init();
                }
            }
        }

        private void ButtonActual_OnClick(object sender, RoutedEventArgs e)
        {
            DoubleAnimation doubleAnimation = AnimationHelper.CreateAnimation(1.0);
            doubleAnimation.FillBehavior = FillBehavior.Stop;
            _imgActualScale = 1.0;
            doubleAnimation.Completed += delegate
            {
                ImageScale = 1.0;
                _canMoveX = ImageWidth > base.ActualWidth;
                _canMoveY = ImageHeight > base.ActualHeight;
            };
            Thickness thickness = new Thickness((base.ActualWidth - ImageOriWidth) / 2.0, (base.ActualHeight - ImageOriHeight) / 2.0, 0.0, 0.0);
            ThicknessAnimation thicknessAnimation = AnimationHelper.CreateAnimation(thickness);
            thicknessAnimation.FillBehavior = FillBehavior.Stop;
            _imgActualMargin = thickness;
            thicknessAnimation.Completed += delegate
            {
                ImageMargin = thickness;
            };
            BeginAnimation(ImageScaleProperty, doubleAnimation);
            BeginAnimation(ImageMarginProperty, thicknessAnimation);
        }

        private void ButtonReduce_OnClick(object sender, RoutedEventArgs e)
        {
            ScaleImg(isEnlarge: false);
        }

        private void ButtonEnlarge_OnClick(object sender, RoutedEventArgs e)
        {
            ScaleImg(isEnlarge: true);
        }

        private void ButtonRotateLeft_OnClick(object sender, RoutedEventArgs e)
        {
            RotateImg(_imgActualRotate - 90.0);
        }

        private void ButtonRotateRight_OnClick(object sender, RoutedEventArgs e)
        {
            RotateImg(_imgActualRotate + 90.0);
        }

        private void ButtonSave_OnClick(object sender, RoutedEventArgs e)
        {
            if (ImageSource == null)
            {
                return;
            }

            SaveFileDialog.FileName = $"{DateTime.Now:yyyy-M-d-h-m-s.fff}";
            if (SaveFileDialog.ShowDialog() != true)
            {
                return;
            }

            using FileStream stream = new FileStream(SaveFileDialog.FileName, FileMode.Create, FileAccess.Write);
            PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
            pngBitmapEncoder.Frames.Add(BitmapFrame.Create(ImageSource));
            pngBitmapEncoder.Save(stream);
        }

        //private void ButtonWindowsOpen_OnClick(object sender, RoutedEventArgs e)
        //{
        //    Uri uri = Uri;
        //    if ((object)uri != null)
        //    {
        //        _imageBrowser?.Close();
        //        _imageBrowser = new ImageBrowser(uri);
        //        _imageBrowser.Show();
        //    }
        //}

        protected override void OnMouseMove(MouseEventArgs e)
        {
            MoveImg();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            ShowBorderBottom = false;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            ScaleImg(e.Delta > 0);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            OnRenderSizeChanged();
        }

        private void OnRenderSizeChanged()
        {
            if (!(ImageWidth < 0.001) && !(ImageHeight < 0.001))
            {
                _canMoveX = true;
                _canMoveY = true;
                double left = ImageMargin.Left;
                double top = ImageMargin.Top;
                if (ImageWidth <= base.ActualWidth)
                {
                    _canMoveX = false;
                    left = (base.ActualWidth - ImageWidth) / 2.0;
                }

                if (ImageHeight <= base.ActualHeight)
                {
                    _canMoveY = false;
                    top = (base.ActualHeight - ImageHeight) / 2.0;
                }

                ImageMargin = new Thickness(left, top, 0.0, 0.0);
                _imgActualMargin = ImageMargin;
            }
        }

        private void ImageMain_OnMouseDown(object sender, ExecutedRoutedEventArgs e)
        {
            _imgMouseDownPoint = Mouse.GetPosition(_panelMain);
            _imgMouseDownMargin = ImageMargin;
            _imgIsMouseDown = true;
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            _imgIsMouseDown = false;
        }
        
        private void ScaleImg(bool isEnlarge)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                return;
            }

            double imageWidth = ImageWidth;
            double imageHeight = ImageHeight;
            double num = (isEnlarge ? (_imgActualScale + 0.1) : (_imgActualScale - 0.1));
            if (Math.Abs(num) < MinScale)
            {
                num = MinScale;
            }
            else if (Math.Abs(num) > MaxScale)
            {
                num = MaxScale;
            }

            if (Math.Abs(num - _imgActualScale) < 0.001)
            {
                return;
            }
            ImageScale = num;

            bool imageFitsWidth = ImageWidth <= base.ActualWidth;
            bool imageFitsHeight = ImageHeight <= base.ActualHeight;
            bool imageFitsCompletely = imageFitsWidth && imageFitsHeight;

            Point position = Mouse.GetPosition(_panelMain);
            Point point = new Point(position.X - _imgActualMargin.Left, position.Y - _imgActualMargin.Top);
            double num2 = 0.5 * _scaleInternalWidth;
            double num3 = 0.5 * _scaleInternalHeight;

            //_canMoveX = ImageWidth > base.ActualWidth;
            //_canMoveY = ImageHeight > base.ActualHeight;

            if (ImageWidth > base.ActualWidth)
            {
                _canMoveX = true;
                if (ImageHeight > base.ActualHeight)
                {
                    _canMoveY = true;
                    num2 = point.X / imageWidth * _scaleInternalWidth;
                    num3 = point.Y / imageHeight * _scaleInternalHeight;
                }
                else
                {
                    _canMoveY = false;
                }
            }
            else
            {
                _canMoveY = ImageHeight > base.ActualHeight;
                _canMoveX = false;
            }

            Thickness thickness;
            if (isEnlarge)
            {
                thickness = new Thickness(_imgActualMargin.Left - num2, _imgActualMargin.Top - num3, 0.0, 0.0);
            }
            else
            {
                double left = _imgActualMargin.Left + num2;
                double top = _imgActualMargin.Top + num3;
                double num4 = ImageWidth - base.ActualWidth;
                double num5 = ImageHeight - base.ActualHeight;
                if (Math.Abs(ImageMargin.Left) < 0.001)
                {
                    left = _imgActualMargin.Left /*+ _borderMove.Margin.Left / (_canvasSmallImg.ActualWidth - _borderMove.Width)*/ * _scaleInternalWidth;
                }

                if (Math.Abs(ImageMargin.Top) < 0.001)
                {
                    top = _imgActualMargin.Top /*+ _borderMove.Margin.Top / (_canvasSmallImg.ActualHeight - _borderMove.Height)*/ * _scaleInternalHeight;
                }

                if (num4 < 0.001)
                {
                    left = (base.ActualWidth - ImageWidth) / 2.0;
                }

                if (num5 < 0.001)
                {
                    top = (base.ActualHeight - ImageHeight) / 2.0;
                }

                thickness = new Thickness(left, top, 0.0, 0.0);
                
            }

            ImageMargin = thickness;
            _imgActualScale = num;
            _imgActualMargin = thickness;
        }
        private void RotateImg(double rotate)
        {
            _imgActualRotate = rotate;
            _isOblique = ((int)_imgActualRotate - 90) % 180 == 0;
            Init();
            //InitBorderSmall();
            DoubleAnimation doubleAnimation = AnimationHelper.CreateAnimation(rotate);
            doubleAnimation.Completed += delegate
            {
                ImageRotate = rotate;
            };
            doubleAnimation.FillBehavior = FillBehavior.Stop;
            BeginAnimation(ImageRotateProperty, doubleAnimation);
        }

        private MouseButtonState GetMouseButtonState()
        {
            return MoveGesture.MouseAction switch
            {
                MouseAction.LeftClick => Mouse.LeftButton,
                MouseAction.RightClick => Mouse.RightButton,
                MouseAction.MiddleClick => Mouse.MiddleButton,
                _ => Mouse.LeftButton,
            };
        }
        private void MoveImg()
        {
            _imgCurrentPoint = Mouse.GetPosition(_panelMain);
            ShowCloseButton = _imgCurrentPoint.Y < 200.0;
            ShowBorderBottom = _imgCurrentPoint.Y > base.ActualHeight - 200.0;
            if (GetMouseButtonState() == MouseButtonState.Released || !_imgIsMouseDown)
            {
                return;
            }

            double num = _imgCurrentPoint.X - _imgMouseDownPoint.X;
            double num2 = _imgCurrentPoint.Y - _imgMouseDownPoint.Y;
            double num3 = _imgMouseDownMargin.Left;
            if (ImageWidth > base.ActualWidth)
            {
                num3 = _imgMouseDownMargin.Left + num;
                if (num3 >= 0.0)
                {
                    num3 = 0.0;
                }
                else if (0.0 - num3 + base.ActualWidth >= ImageWidth)
                {
                    num3 = base.ActualWidth - ImageWidth;
                }

                _canMoveX = true;
            }

            double num4 = _imgMouseDownMargin.Top;
            if (ImageHeight > base.ActualHeight)
            {
                num4 = _imgMouseDownMargin.Top + num2;
                if (num4 >= 0.0)
                {
                    num4 = 0.0;
                }
                else if (0.0 - num4 + base.ActualHeight >= ImageHeight)
                {
                    num4 = base.ActualHeight - ImageHeight;
                }

                _canMoveY = true;
            }

            ImageMargin = new Thickness(num3, num4, 0.0, 0.0);
            _imgActualMargin = ImageMargin;
        }
        private static void OnMoveGestureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ImageViewerCC)d).OnMoveGestureChanged((MouseGesture)e.NewValue);
        }

        private void OnMoveGestureChanged(MouseGesture newValue)
        {
            base.InputBindings.Remove(_mouseMoveBinding);
            _mouseMoveBinding = new MouseBinding(ControlCommands.MouseMove, newValue);
            base.InputBindings.Add(_mouseMoveBinding);
        }

        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ImageViewerCC)d).OnImageSourceChanged();
        }

        private void OnImageSourceChanged()
        {
            Init();
        }

        //private static void OnUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    ((ImageViewerCC)d).OnUriChanged((Uri)e.NewValue);
        //}

        //private void OnUriChanged(Uri newValue)
        //{
        //    if ((object)newValue != null)
        //    {
        //        Source = GetBitmapFrame(newValue).;
        //        ImgPath = newValue.AbsolutePath;
        //        if (File.Exists(ImgPath))
        //        {
        //            ImgSize = new FileInfo(ImgPath).Length;
        //        }
        //    }
        //    else
        //    {
        //        Source = null;
        //        ImgPath = string.Empty;
        //    }

        //    static BitmapFrame GetBitmapFrame(Uri source)
        //    {
        //        try
        //        {
        //            return BitmapFrame.Create(source);
        //        }
        //        catch
        //        {
        //            return null;
        //        }
        //    }
        //}
    }
}
