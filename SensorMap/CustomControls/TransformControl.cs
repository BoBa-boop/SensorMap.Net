using SensorMap.Commands.SensorCommands;
using SensorMap.Interfaces;
using SensorMap.Services;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;
using Control = System.Windows.Controls.Control;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;

namespace SensorMap.CustomControls
{
    public class TransformControl : Control
    {

        /// <summary>
        /// Команда выполнения трансформации объекта
        /// </summary>
        public ICommand? TransformCommand
        {
            get { return (ICommand)GetValue(TransformCommandProperty); }
            set { SetValue(TransformCommandProperty, value); }
        }

        public static readonly DependencyProperty TransformCommandProperty =
            DependencyProperty.Register("TransformCommand", typeof(ICommand), typeof(TransformControl), new PropertyMetadata(null));



        public IMapElement? Target
        {
            get { return (IMapElement)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(IMapElement), typeof(TransformControl), new PropertyMetadata(null));


        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(TransformControl), new PropertyMetadata(0.0));

        private static readonly (string part, HitType hit)[] 
            Parts = 
            { 
            ("PART_LeftTopMarker",HitType.UpLeft),
            ("PART_TopMarker", HitType.Top),
            ("PART_RightTopMarker",HitType.UpRight),
            ("PART_RightCenterMarker",HitType.Right),
            ("PART_RightBottomMarker",HitType.BottomRight),
            ("PART_BottomMarker",HitType.Bottom),
            ("PART_LeftBottomMarker",HitType.BottomLeft),
            ("PART_LeftCenterMarker",HitType.Left)
        };

        static TransformControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TransformControl), new FrameworkPropertyMetadata(typeof(TransformControl)));
        }

        private readonly ITransformObject _service = new TransformObjectService();
        private readonly Dictionary<FrameworkElement, HitType> _handles = new();
        private Canvas? _canvas;
        private HitType _activeHit;
        private FrameworkElement? _activeHandle;
        private Point _startPoint;
        private Rect _startBounds;

        public TransformControl()
        {

        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            foreach (var h in _handles.Keys)
            {
                h.PreviewMouseLeftButtonDown -= Handle_Down;
                h.PreviewMouseMove -= Handle_Move;
                h.PreviewMouseLeftButtonUp -= Handle_Up;
            }

            _handles.Clear();

            foreach (var (part,hit) in Parts)
            {
                if (GetTemplateChild(part) is not FrameworkElement h) continue;
                _handles[h] = hit;
                h.PreviewMouseLeftButtonDown += Handle_Down;
                h.PreviewMouseMove += Handle_Move;
                h.PreviewMouseLeftButtonUp += Handle_Up;
            }
            UpdateCursor();
        }

        private void UpdateCursor()
        {
            foreach (var (h,hit) in _handles)
            {
                h.Cursor = _service.GetCursorForHitType(hit, Angle);
            }
        }

        private void Handle_Move(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_activeHandle == null || !ReferenceEquals(sender, _activeHandle) || Target == null || _canvas == null) return;
            Point p = e.GetPosition(_canvas);
            Rect r = _service.TransformRectangle(_startBounds,_activeHit,p.X - _startPoint.X,p.Y - _startPoint.Y, Angle);

            SetBounds(r);
        }

        private void SetBounds(Rect r)
        {
            if (r.IsEmpty) return;
            {
                
                if ((r.Width > 20) && (r.Height > 20))
                {
                    
                    Canvas.SetLeft(Target.Element, r.X);
                    Canvas.SetTop(Target.Element, r.Y);
                }
            }
        }

        private void Handle_Up(object sender, MouseButtonEventArgs e)
        {
            if (_activeHandle == null) return;

            _activeHandle.ReleaseMouseCapture();
            _activeHandle = null;
            e.Handled = true;

            if (Target != null)
            {
                var command = new TransformationSensors(new[] { Target.MapData }, new IMapElement[] { Target }, p => p);
                if (TransformCommand?.CanExecute(command) == true)
                {
                    TransformCommand.Execute(command);
                }
            }
        }

       

        private void Handle_Down(object sender, MouseButtonEventArgs e)
        {
            if (Target == null) return;
            _canvas ??= _service.GetParentCanvas(this);
            if(_canvas == null ) return;
            _activeHandle = (FrameworkElement)sender;
            _activeHit = _handles[_activeHandle];
            _startPoint = e.GetPosition(_canvas);
            _startBounds = Target.GetCustomBounds();

            _activeHandle.CaptureMouse();
            e.Handled = true;
        }
    }
}
