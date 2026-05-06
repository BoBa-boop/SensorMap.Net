using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using ReactiveUI;
using SensorMap.Commands.SensorCommands;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using SensorMap.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;
using Control = System.Windows.Controls.Control;
using Image = System.Windows.Controls.Image;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;

namespace SensorMap.CustomControls
{
    [TemplatePart(Name = "PART_Sensor", Type = typeof(Border))]
    [TemplatePart(Name = "PART_Address", Type = typeof(HandyControl.Controls.TextBox))]
    public class CustomSensor : Control
    {
        #region Dependency Properties


        public int Id
        {
            get { return (int)GetValue(IdProperty); }
            set { SetValue(IdProperty, value); }
        }
        public static readonly DependencyProperty IdProperty =
            DependencyProperty.Register("Id", typeof(int), typeof(CustomSensor), new PropertyMetadata(0));


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
                new PropertyMetadata(new SolidColorBrush(Colors.Red)));
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
            //// Обновляем Width/Height если используете отдельные свойства
            ////sensor.Width = newRect.Width;
            ////sensor.Height = newRect.Height;
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
        /// <summary>
        /// Команда от CustomSensor
        /// </summary>
        public ICommand SensorCommand
        {
            get { return (ICommand)GetValue(SensorCommandProperty); }
            set { SetValue(SensorCommandProperty, value); }
        }
        public static readonly DependencyProperty SensorCommandProperty =
            DependencyProperty.Register("SensorCommand", typeof(ICommand),
                typeof(CustomSensor),
                new PropertyMetadata(null));



        public static CustomSensor SelectedCustomSensor;

        public CustomSensor SelectedSensor
        {
            get { return (CustomSensor)GetValue(SelectedSensorProperty); }
            set { SetValue(SelectedSensorProperty, value); }
        }
        public static readonly DependencyProperty SelectedSensorProperty =
            DependencyProperty.Register("SelectedSensor", typeof(CustomSensor),
                typeof(CustomSensor),
                new PropertyMetadata(null));

        //private static void SelectedSensorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    var control = (CustomSensor)d;
        //    if (control.IsSelected)
        //    {
        //        MessageBox.Show("asdasd");
        //    }
        //}




        #endregion

        private readonly ITransformObject _transformService;
        HitType MouseHitType = HitType.None;
        private Point LastPoint;
        private bool IsTransformed = false;
        private bool DragInProgress = false;
        private  Canvas _canvas;
        private System.Windows.Controls.Image _image;
        private HandyControl.Controls.TextBox _textBoxAddress;
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
            if ( _canvas != null )
            {
                _textBoxAddress = GetTemplateChild("PART_Address") as HandyControl.Controls.TextBox;
                ChangeStateActions();
            }

        }

        private void ChangeStateActions()
        {
            this.IsSelected = false;
            if (IsEditMode)
            {
                this.MouseDown += OnMouseDown;
                this.MouseMove += OnSensorMouseMove;
                _canvas.MouseMove += OnMouseMove;
                _canvas.MouseUp += OnMouseUp;
                this.PreviewKeyDown += OnPreviewKeyDown;
            }
            else
            {
                this.MouseDown -= OnMouseDown;
                this.MouseMove -= OnSensorMouseMove;
                _canvas.MouseMove -= OnMouseMove;
                _canvas.MouseUp -= OnMouseUp;
                this.PreviewKeyDown -= OnPreviewKeyDown;
            }
        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Delete && IsSelected)
                SensorCommand.Execute(this.SensorData);
        }

        private void OnSensorMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(this.IsSelected && !DragInProgress)
            {
                Rect rect = new Rect(Canvas.GetLeft(this), Canvas.GetTop(this), this.CustomBounds.Width, this.CustomBounds.Height);
                MouseHitType = _transformService.GetHitType(rect, Mouse.GetPosition(_canvas));
                this.Cursor = _transformService.GetCursorForHitType(MouseHitType);
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DragInProgress == false) return;
            double screenX = Canvas.GetLeft(this);
            double screenY = Canvas.GetTop(this);

            Point worldPoint = _transformService.ScreenToWorld(new Point(screenX, screenY), MapProperties.GetViewMatrix(this));

            if(IsTransformed)
            {
                var command = new TransformationSensor(this, CustomBounds, (x) => _transformService.WorldToScreen(worldPoint, MapProperties.GetViewMatrix(this)));
                TransformCommand.Execute(command);
            }

            e.Handled = true;
            DragInProgress = false;
            IsTransformed = false;
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(SelectedCustomSensor!=null)
            {
                if (DragInProgress)                
                {
                    // See how much the mouse has moved.
                    Point point = Mouse.GetPosition(_canvas);
                    double offset_x = point.X - LastPoint.X;
                    double offset_y = point.Y - LastPoint.Y;

                    // Get the rectangle's current position.
                    double new_x = Canvas.GetLeft(this);
                    double new_y = Canvas.GetTop(this);
                    double new_width = this.CustomBounds.Width;
                    double new_height = this.CustomBounds.Height;

                    // Update the rectangle.
                    switch (MouseHitType)
                    {
                        case HitType.Body:
                            new_x += offset_x;
                            new_y += offset_y;
                            break;
                        case HitType.UpLeft:
                            new_x += offset_x;
                            new_y += offset_y;
                            new_width -= offset_x;
                            new_height -= offset_y;
                            break;
                        case HitType.UpRight:
                            new_y += offset_y;
                            new_width += offset_x;
                            new_height -= offset_y;
                            break;
                        case HitType.BottomRight:
                            new_width += offset_x;
                            new_height += offset_y;
                            break;
                        case HitType.BottomLeft:
                            new_x += offset_x;
                            new_width -= offset_x;
                            new_height += offset_y;
                            break;
                        case HitType.Left:
                            new_x += offset_x;
                            new_width -= offset_x;
                            break;
                        case HitType.Right:
                            new_width += offset_x;
                            break;
                        case HitType.Bottom:
                            new_height += offset_y;
                            break;
                        case HitType.Top:
                            new_y += offset_y;
                            new_height -= offset_y;
                            break;
                    }
                    if((new_x > 1 && new_x < _image.ActualWidth-new_width) && (new_y>1 && new_y < _image.ActualHeight-new_height))
                    {
                        // Don't use negative width or height.
                        if ((new_width > 17) && (new_height > 17))
                        {
                            Canvas.SetLeft(this, new_x);
                            Canvas.SetTop(this, new_y);

                            this.CustomBounds = new Rect(new_x, new_y, new_width, new_height);

                            // Save the mouse's new location.
                            LastPoint = point;
                            IsTransformed = true;
                        }
                    }
                }
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCustomSensor != null && SelectedCustomSensor != this)
            {
                SelectedSensor = null;
                SelectedCustomSensor.IsSelected = false;
                //SelectedCustomSensor.CustBorderBrush = Brushes.Black;
            }
            if (MouseHitType!=HitType.None)
            {
                LastPoint = Mouse.GetPosition(_canvas);
                DragInProgress = true;
            }
            SelectedCustomSensor = this;
            SelectedSensor = this;
            SelectedCustomSensor.IsSelected = true;
            //SelectedSensor = SelectedCustomSensor.SensorData;
            this.Focus();
        }

        
        
        private void SelectedChanged()
        {
            SelectedCustomSensor = this.IsSelected ? this:null;
            CustBorderBrush = IsSelected ? Brushes.DarkGreen : Brushes.Black;
            MouseHitType = IsSelected ? MouseHitType : HitType.None;
            this.Cursor = _transformService.GetCursorForHitType(MouseHitType);
        }
    }
}
