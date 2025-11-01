using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SensorMap.CustomControls
{
    /// <summary>
    /// Логика взаимодействия для SensorDrag_Drop.xaml
    /// </summary>
    public partial class SensorDrag_Drop : UserControl
    {
        public SensorDrag_Drop()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty IsChildHitTestVisibleProperty =
            DependencyProperty.Register("IsChildHitTestVisible", typeof(bool), typeof(SensorDrag_Drop),
                new PropertyMetadata(true));

        public bool IsChildHitTestVisible
        {
            get { return (bool)GetValue(IsChildHitTestVisibleProperty); }
            set { SetValue(IsChildHitTestVisibleProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Brush), typeof(SensorDrag_Drop),
                new PropertyMetadata(Brushes.Red));

        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty SensorDropCommandProperty =
            DependencyProperty.Register("SensorDropCommand", typeof(ICommand), typeof(SensorDrag_Drop),
                new PropertyMetadata(null));

        public ICommand SensorDropCommand
        {
            get { return (ICommand)GetValue(SensorDropCommandProperty); }
            set { SetValue(SensorDropCommandProperty, value); }
        }

        public static readonly DependencyProperty SensorRemoveCommandProperty =
            DependencyProperty.Register("SensorRemoveCommand", typeof(ICommand), typeof(SensorDrag_Drop),
                new PropertyMetadata(null));

        public ICommand SensorRemoveCommand
        {
            get { return (ICommand)GetValue(SensorRemoveCommandProperty); }
            set { SetValue(SensorRemoveCommandProperty, value); }
        }

        public static readonly DependencyProperty RemoveSensorNameProperty =
            DependencyProperty.Register("RemoveSensorName", typeof(string), typeof(SensorDrag_Drop),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string RemoveSensorName
        {
            get { return (string)GetValue(RemoveSensorNameProperty); }
            set { SetValue(RemoveSensorNameProperty, value); }
        }

        private void Sensor_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsChildHitTestVisible = false;
                DragDrop.DoDragDrop(sensor, new DataObject(DataFormats.Serializable, sensor), DragDropEffects.Move);
                IsChildHitTestVisible = true;
            }
        }

        private void Canvas_DragLeave(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);

            if (data is FrameworkElement element)
            {
                canvas.Children.Remove(element);
                RemoveSensorName = element.Name;
                SensorRemoveCommand?.Execute(null);
            }
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            // Save the location to a database?
            SensorDropCommand?.Execute(null);
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);

            if (data is UIElement element)
            {
                Point dropPosition = e.GetPosition(canvas);
                Canvas.SetLeft(element, dropPosition.X);
                Canvas.SetTop(element, dropPosition.Y);

                if (!canvas.Children.Contains(element))
                {
                    canvas.Children.Add(element);
                }
            }
        }

        //public static readonly DependencyProperty DragProperty =
        //   DependencyProperty.RegisterAttached("Drag", typeof(bool), typeof(SensorDrag_Drop),
        //       new PropertyMetadata(false, OnDragChanged));

        //public static readonly DependencyProperty DragContainerProperty =
        //    DependencyProperty.RegisterAttached("DragContainer", typeof(FrameworkElement),
        //        typeof(SensorDrag_Drop), new PropertyMetadata(null));

        //public static readonly DependencyProperty SensorDropCommandProp =
        //    DependencyProperty.Register("SensorDropCommand", typeof(ICommand), typeof(SensorDrag_Drop), new PropertyMetadata(null));



        //public static bool GetDrag(DependencyObject obj) => (bool)obj.GetValue(DragProperty);
        //public static void SetDrag(DependencyObject obj, bool value) => obj.SetValue(DragProperty, value);

        //public static FrameworkElement GetDragContainer(DependencyObject obj) =>
        //    (FrameworkElement)obj.GetValue(DragContainerProperty);
        //public static void SetDragContainer(DependencyObject obj, FrameworkElement value) =>
        //    obj.SetValue(DragContainerProperty, value);
        //public ICommand SensorDropCommand
        //{
        //    get { return (ICommand)GetValue(SensorDropCommandProp); }
        //    set { SetValue(SensorDropCommandProp, value); }
        //}

        //private static void OnDragChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //{
        //    if (d is UIElement element)
        //    {
        //        if ((bool)e.NewValue)
        //        {
        //            element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
        //            element.MouseLeftButtonUp += Element_MouseLeftButtonUp;
        //            element.MouseMove += Element_MouseMove;
        //        }
        //        else
        //        {
        //            element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
        //            element.MouseLeftButtonUp -= Element_MouseLeftButtonUp;
        //            element.MouseMove -= Element_MouseMove;
        //        }
        //    }
        //}

        //private static bool _isDragging = false;
        //private static Point _startPoint;
        //private static UIElement _draggedElement;

        //private static void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    _isDragging = true;
        //    _draggedElement = (UIElement)sender;
        //    _startPoint = e.GetPosition(_draggedElement);
        //    _draggedElement.CaptureMouse();
        //}

        //private static void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    _isDragging = false;
        //    _draggedElement?.ReleaseMouseCapture();
        //    _draggedElement = null;
        //}

        //private static void Element_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (!_isDragging || _draggedElement == null) return;

        //    var container = GetDragContainer(_draggedElement);
        //    if (container == null) return;

        //    var currentPosition = e.GetPosition(container);

        //    // Вычисляем новые координаты
        //    double newX = currentPosition.X - 5;
        //    double newY = currentPosition.Y - 5;

        //    // Ограничиваем перемещение границами контейнера
        //    newX = Math.Max(_draggedElement.RenderSize.Width / 2.0, Math.Min(newX, container.ActualWidth - _draggedElement.RenderSize.Width));
        //    newY = Math.Max(_draggedElement.RenderSize.Height / 2.0, Math.Min(newY, container.ActualHeight - _draggedElement.RenderSize.Height));

        //    // Применяем новые координаты
        //    Canvas.SetLeft(_draggedElement, newX);
        //    Canvas.SetTop(_draggedElement, newY);
        //}
    }
}
