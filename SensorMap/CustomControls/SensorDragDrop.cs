using System;
using System.Collections.Generic;
using System.Linq;
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

namespace SensorMap.CustomControls
{

    [TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
    [TemplatePart(Name = "PART_Sensor", Type = typeof(Ellipse))]
    public class SensorDragDrop : Control
    {
        static SensorDragDrop()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SensorDragDrop),
                new FrameworkPropertyMetadata(typeof(SensorDragDrop)));
        }

        private Canvas _canvas;
        private Ellipse _sensor;
        private bool _isDragging = false;

        #region Dependency Properties

        public static readonly DependencyProperty XProperty =
            DependencyProperty.Register("X", typeof(double), typeof(SensorDragDrop),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        public static readonly DependencyProperty YProperty =
            DependencyProperty.Register("Y", typeof(double), typeof(SensorDragDrop),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        public static readonly DependencyProperty SensorDropCommandProperty =
            DependencyProperty.Register("SensorDropCommand", typeof(ICommand), typeof(SensorDragDrop));

        public ICommand SensorDropCommand
        {
            get { return (ICommand)GetValue(SensorDropCommandProperty); }
            set { SetValue(SensorDropCommandProperty, value); }
        }

        public static readonly DependencyProperty SensorRemoveCommandProperty =
            DependencyProperty.Register("SensorRemoveCommand", typeof(ICommand), typeof(SensorDragDrop));

        public ICommand SensorRemoveCommand
        {
            get { return (ICommand)GetValue(SensorRemoveCommandProperty); }
            set { SetValue(SensorRemoveCommandProperty, value); }
        }

        public static readonly DependencyProperty RemoveSensorNameProperty =
            DependencyProperty.Register("RemoveSensorName", typeof(string), typeof(SensorDragDrop));

        public string RemoveSensorName
        {
            get { return (string)GetValue(RemoveSensorNameProperty); }
            set { SetValue(RemoveSensorNameProperty, value); }
        }
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _canvas = GetTemplateChild("PART_Canvas") as Canvas;
            _sensor = GetTemplateChild("PART_Sensor") as Ellipse;

            if (_canvas != null)
            {
                _canvas.AllowDrop = true;
                _canvas.DragOver += Canvas_DragOver;
                _canvas.Drop += Canvas_Drop;
                _canvas.DragLeave += Canvas_DragLeave;
            }

            if (_sensor != null)
            {
                _sensor.MouseLeftButtonDown += Sensor_MouseLeftButtonDown;
                _sensor.MouseMove += Sensor_MouseMove;
                _sensor.MouseLeave += _sensor_MouseLeave;
                _sensor.MouseEnter += _sensor_MouseEnter;
                _sensor.MouseLeftButtonUp += Sensor_MouseLeftButtonUp;
            }
        }

        private void _sensor_MouseEnter(object sender, MouseEventArgs e)
        {
            _sensor.IsHitTestVisible = true;
            _canvas.IsHitTestVisible = true;
        }

        private void _sensor_MouseLeave(object sender, MouseEventArgs e)
        {
            _sensor.IsHitTestVisible = false;
            _canvas.IsHitTestVisible = false;
        }

        private void Sensor_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            e.Handled = false; // Важно: позволяем событию всплывать дальше
        }

        private void Sensor_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;

                // Сохраняем текущие координаты перед началом перетаскивания
                UpdateCoordinates();
                _sensor.IsHitTestVisible = false;
                DragDrop.DoDragDrop(_sensor, new DataObject(DataFormats.Serializable, _sensor), DragDropEffects.Move);
                _isDragging = false;
                _sensor.IsHitTestVisible = true;
            }

            e.Handled = false; // Позволяем событию всплывать
        }

        private void Sensor_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            e.Handled = false;
        }

        private void Canvas_DragOver(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);

            if (data is UIElement element)
            {
                Point dropPosition = e.GetPosition(_canvas);

                // Центрируем сенсор относительно курсора
                double left = dropPosition.X - element.RenderSize.Width / 2;
                double top = dropPosition.Y - element.RenderSize.Height / 2;

                Canvas.SetLeft(element, left);
                Canvas.SetTop(element, top);

                // Обновляем координаты в реальном времени
                X = dropPosition.X;
                Y = dropPosition.Y;

                if (!_canvas.Children.Contains(element))
                {
                    _canvas.Children.Add(element);
                }

                e.Handled = true; // Обрабатываем событие здесь
            }
        }

        private void Canvas_Drop(object sender, DragEventArgs e)
        {
            UpdateCoordinates();
            SensorDropCommand?.Execute(null);
            e.Handled = true;
        }

        private void Canvas_DragLeave(object sender, DragEventArgs e)
        {
            object data = e.Data.GetData(DataFormats.Serializable);

            if (data is FrameworkElement element)
            {
                _canvas.Children.Remove(element);
                RemoveSensorName = element.Name;
                SensorRemoveCommand?.Execute(null);
            }

            e.Handled = true;
        }

        private void UpdateCoordinates()
        {
            if (_sensor != null && _canvas != null)
            {
                Point position = _sensor.TranslatePoint(new Point(_sensor.ActualWidth / 2, _sensor.ActualHeight / 2), _canvas);
                X = position.X;
                Y = position.Y;
            }
        }

        // Метод для обновления позиции сенсора извне
        public void SetPosition(double x, double y)
        {
            if (_sensor != null)
            {
                Canvas.SetLeft(_sensor, x - _sensor.ActualWidth / 2);
                Canvas.SetTop(_sensor, y - _sensor.ActualHeight / 2);
                UpdateCoordinates();
            }
        }
    }
}

