using CommunityToolkit.Mvvm.Input;
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
using Brushes = System.Windows.Media.Brushes;
using Control = System.Windows.Controls.Control;
using Cursor = System.Windows.Input.Cursor;
using Cursors = System.Windows.Input.Cursors;
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
            DependencyProperty.Register("Sensor", typeof(SensorAssignments), typeof(CustomSensor));
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

        public object ViewModel
        {
            get { return (object)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(object), typeof(CustomSensor), new PropertyMetadata(null));


        public Rect CustomBounds
        {
            get => (Rect)GetValue(BoundsProperty);
            set => SetValue(BoundsProperty, value);
        }
        public static readonly DependencyProperty BoundsProperty = DependencyProperty.Register("CustomBounds", typeof(Rect),
            typeof(CustomSensor),new PropertyMetadata(new Rect(0,0,30,30)));
        
        
        #endregion

        private readonly ITransformObject _transformService;
        HitType MouseHitType = HitType.None;
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

            if (_canvas == null)
            {
                _canvas = _transformService.GetParentCanvas(this);
            } 
            if( _canvas != null ) 
            {
                CustomBounds = new Rect(SensorData.X, SensorData.Y, 30, 30);
                if (IsEditMode)
                {
                    this.MouseDown += OnMouseDown;
                    this.MouseMove += OnMouseMove;
                    this.MouseUp += OnMouseUp;
                }
            }
            
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            // 1. Получаем текущие экранные координаты
            double screenX = Canvas.GetLeft(this);
            double screenY = Canvas.GetTop(this);

            // 2. Конвертируем в мировые
            Point worldPoint = _transformService.ScreenToWorld(new Point(screenX, screenY), MapProperties.GetViewMatrix(this));

            // 5. Создаем команду с МИРОВЫМИ координатами
            if (ViewModel != null && ViewModel is MechanismVM vm && worldPoint.X > 1 && worldPoint.Y > 1)
                vm.MoveSensorCommand(this, worldPoint, SensorData, (x) => _transformService.WorldToScreen(worldPoint, MapProperties.GetViewMatrix(this)));

            e.Handled = true;
            DragInProgress = false;
        }

        private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this != null&&this.IsSelected)
            {
                if (!DragInProgress)
                {
                    MouseHitType = _transformService.GetHitType(CustomBounds,Mouse.GetPosition(_canvas));
                    Mouse.OverrideCursor = _transformService.GetCursorForHitType(MouseHitType);
                }
                else
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

                    // Don't use negative width or height.
                    if ((new_width > 18) && (new_height > 18))
                    {
                        Canvas.SetLeft(this, new_x);
                        Canvas.SetTop(this, new_y);
                        this.CustomBounds = new Rect(new_x, new_y, new_width, new_height);

                        // Save the mouse's new location.
                        LastPoint = point;
                    }
                }
            }
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MouseHitType == HitType.None) return;

            LastPoint = Mouse.GetPosition(_canvas);
            DragInProgress = true;
        }

        
        
        private void SelectedChanged()
        {
            CustBorderBrush = IsSelected ? Brushes.DarkGreen : Brushes.Black;
        }
    }
}
