using SensorMap.Model;
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
//https://www.interestprograms.ru/source-codes-peremeshchenie-ehlementov-myshyu-v-okne
namespace SensorMap.CustomControls.SensorDrag
{
    /// <summary>
    /// Логика взаимодействия для SensorDragDrop.xaml
    /// </summary>
    public partial class SensorDragDrop : UserControl
    {
        public SensorDragDrop()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IsChildHitTestVisibleProperty =
           DependencyProperty.Register("IsChildHitTestVisible", typeof(bool), typeof(SensorDragDrop),
               new PropertyMetadata(true));

        public bool IsChildHitTestVisible
        {
            get { return (bool)GetValue(IsChildHitTestVisibleProperty); }
            set { SetValue(IsChildHitTestVisibleProperty, value); }
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Brush), typeof(SensorDragDrop),
                new PropertyMetadata(Brushes.Black));

        public Brush Color
        {
            get { return (Brush)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public static readonly DependencyProperty SensorDropCommandProperty =
            DependencyProperty.Register("SensorDropCommand", typeof(ICommand), typeof(SensorDragDrop),
                new PropertyMetadata(null));

        public ICommand SensorDropCommand
        {
            get { return (ICommand)GetValue(SensorDropCommandProperty); }
            set { SetValue(SensorDropCommandProperty, value); }
        }

        public static readonly DependencyProperty SensorRemoveCommandProperty =
            DependencyProperty.Register("SensorRemoveCommand", typeof(ICommand), typeof(SensorDragDrop),
                new PropertyMetadata(null));

        public ICommand SensorRemoveCommand
        {
            get { return (ICommand)GetValue(SensorRemoveCommandProperty); }
            set { SetValue(SensorRemoveCommandProperty, value); }
        }

        public static readonly DependencyProperty RemoveSensorNameProperty =
            DependencyProperty.Register("RemoveSensorName", typeof(string), typeof(SensorDragDrop),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string RemoveSensorName
        {
            get { return (string)GetValue(RemoveSensorNameProperty); }
            set { SetValue(RemoveSensorNameProperty, value); }
        }

       

        private void sensor_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                IsChildHitTestVisible = false;
                DragDrop.DoDragDrop(sensor, new DataObject(DataFormats.Serializable, sensor), DragDropEffects.Move);
                IsChildHitTestVisible = true;
            }
        }
        // Счётчик z-Index позиции 
        int countZ = 0;
        bool _canMove = false;
        Point _offsetPoint = new(0, 0);
        private void FF_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Разрешаем перемещение.
            _canMove = true;
            // Каждое перемещение будет увеличивать zIndex элемента.
            // Размера типа Int достаточно для очень длительной работы приложения.
            countZ++;

            // Объект вызывающий событие приводим к универсальному для всех элементов типу.
            //  Таким образом можно тестирован многие элементы не корректируя код.
            FrameworkElement ffElement = (FrameworkElement)sender;
            // Поднимаем над всеми активный элемент.
            Grid.SetZIndex(ffElement, countZ);

            //  Позиция курсора в начале движения.
            Point posCursor = e.MouseDevice.GetPosition(this);
            // Значения смещения позиции курсора мыши относительно 
            // левого и верхнего края элемента.
            _offsetPoint =
                new Point(posCursor.X - ffElement.Margin.Left, posCursor.Y - ffElement.Margin.Top);

            // Захват устройства мышь предотвращает отрыв
            // курсора от элемента при резком движении мыши.
            e.MouseDevice.Capture(ffElement);
        }

        private void FF_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_canMove == true)
            {
                FrameworkElement ffElement = (FrameworkElement)sender;
                // Среди множества элементов перемещаться будет только выбранный.
                if (e.MouseDevice.Captured == ffElement)
                {
                    Point p = e.MouseDevice.GetPosition(this);

                    Thickness margin = new(p.X - _offsetPoint.X, p.Y - _offsetPoint.Y, 0, 0);
                    ffElement.Margin = margin;
                }
            }
        }

        private void FF_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _canMove = false;
            // Освобождаем устройство мышь
            e.MouseDevice.Capture(null);
        }

    }
}
