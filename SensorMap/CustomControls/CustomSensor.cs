using CommunityToolkit.Mvvm.Input;
using SensorMap.Commands.SensorCommands;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.Services;
using SensorMap.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Brushes = System.Windows.Media.Brushes;
using Control = System.Windows.Controls.Control;
using Cursor = System.Windows.Input.Cursor;
using Cursors = System.Windows.Input.Cursors;
using MessageBox = System.Windows.MessageBox;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace SensorMap.CustomControls
{
    [TemplatePart(Name = "PART_Sensor", Type = typeof(Border))]
    public class CustomSensor : Control
    {
        #region Dependency Properties
        public SensorAssignments SensorData
        {
            get { return (SensorAssignments)GetValue(SensorProperty); }
            set { SetValue(SensorProperty, value); }
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

        public char? Letter
        {
            get { return (char)GetValue(LetterProperty); }
            set { SetValue(LetterProperty, value); }
        }
        public static readonly DependencyProperty LetterProperty =
            DependencyProperty.Register("Letter", typeof(char), typeof(CustomSensor), new PropertyMetadata('-'));
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
        public bool ShowAddresses
        {
            get { return (bool)GetValue(ShowAddressesProperty); }
            set { SetValue(ShowAddressesProperty, value);}
        }
        public static readonly DependencyProperty ShowAddressesProperty = DependencyProperty.Register("ShowAddresses", typeof(bool),
            typeof(CustomSensor),
            new PropertyMetadata(false));
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
            DependencyProperty.Register("IsEditMode", typeof(bool), typeof(CustomSensor), new PropertyMetadata(false,OnIsEditPropertyChanged));

        private static void OnIsEditPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (CustomSensor)d;
            if (e.NewValue != null && e.NewValue is bool value)
            {
                control.IsEditMode = value;
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
            // Обновляем Width/Height если используете отдельные свойства
            sensor.Width = newRect.Width;
            sensor.Height = newRect.Height;
        }
        
        public ICommand TransformSensorCommand
        {
            get { return (ICommand)GetValue(TransformSensorCommandProperty); }
            set { SetValue(TransformSensorCommandProperty, value); }
        }
        public static readonly DependencyProperty TransformSensorCommandProperty =
            DependencyProperty.Register("TransformSensorCommand", typeof(ICommand),
                typeof(CustomSensor),
                new PropertyMetadata(null));


        #endregion

        private readonly ITransformObject _transformService;
        HitType MouseHitType = HitType.None;
        private UIElement _selectedSensor;
        private Point LastPoint;
        private bool DragInProgress = false;
        private  Canvas _canvas;

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
            if (SensorData!=null&& SensorData.Sensor != null && SensorData.Sensor.SensorType!=null)
                Letter = SensorData.Sensor.SensorType.Name.ToUpper().First();
            
            _canvas = _transformService.GetParentCanvas(this);
 
            if( _canvas != null ) 
            {
                if (IsEditMode)
                {
                    this.MouseDown += OnMouseDown;
                    this.MouseMove += OnSensorMouseMove;
                    _canvas.MouseMove += OnMouseMove;
                    _canvas.MouseUp += OnMouseUp;
                }
            }
            
        }

        private void OnSensorMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(_selectedSensor!= null && !DragInProgress)
            {
                Rect rect = new Rect(Canvas.GetLeft(_selectedSensor), Canvas.GetTop(_selectedSensor), this.CustomBounds.Width, this.CustomBounds.Height);
                MouseHitType = _transformService.GetHitType(rect, Mouse.GetPosition(_canvas));
                this.Cursor = _transformService.GetCursorForHitType(MouseHitType);
            }
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (DragInProgress == false) return;
            // 1. Получаем текущие экранные координаты
            double screenX = Canvas.GetLeft(this);
            double screenY = Canvas.GetTop(this);

            // 2. Конвертируем в мировые
            Point worldPoint = _transformService.ScreenToWorld(new Point(screenX, screenY), MapProperties.GetViewMatrix(this));

            // 5. Создаем команду с МИРОВЫМИ координатами
            var command = new TransformationSensor(this, CustomBounds, (x) => _transformService.WorldToScreen(worldPoint, MapProperties.GetViewMatrix(this)));
            TransformSensorCommand.Execute(command);

            e.Handled = true;
            DragInProgress = false;
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(_selectedSensor!=null)
            {
                if (DragInProgress)                
                {
                    // See how much the mouse has moved.
                    Point point = Mouse.GetPosition(_canvas);
                    double offset_x = point.X - LastPoint.X;
                    double offset_y = point.Y - LastPoint.Y;

                    // Get the rectangle's current position.
                    double new_x = Canvas.GetLeft(_selectedSensor);
                    double new_y = Canvas.GetTop(_selectedSensor);
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

                    // Don't use negative width or height.
                    if ((new_width > 18) && (new_height > 18))
                    {
                        Canvas.SetLeft(_selectedSensor, new_x);
                        Canvas.SetTop(_selectedSensor, new_y);
                        this.CustomBounds = new Rect(new_x, new_y, new_width, new_height);

                        // Save the mouse's new location.
                        LastPoint = point;
                    }
                }
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(MouseHitType!=HitType.None)
            {
                LastPoint = Mouse.GetPosition(_canvas);
                DragInProgress = true;
            }
            this.CustBorderBrush = Brushes.ForestGreen;
            this.IsSelected = true;
            _selectedSensor = this;
        }

        
        
        private void SelectedChanged()
        {
            CustBorderBrush = IsSelected ? Brushes.DarkGreen : Brushes.Black;
        }
    }
}
