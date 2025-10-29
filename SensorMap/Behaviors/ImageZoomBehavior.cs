using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SensorMap.Behaviors
{
    public class ImageZoomBehavior:Behavior<Image>
    {
        private TransformGroup _transformGroup;
        private TranslateTransform _translateTransform;
        private ScaleTransform _scaleTransform;
        private Point _startPoint;
        private bool _isDragging;
        private Rect _boundsRect;

        private bool _canMoveX;
        private bool _canMoveY;
        private Point _mouseDownPoint;
        private Size _imgSize;


        public static readonly DependencyProperty MinScaleProperty =
            DependencyProperty.Register("MinScale", typeof(double), typeof(ImageZoomBehavior),
                                        new PropertyMetadata(1.0)); 

        public double MinScale
        {
            get { return (double)GetValue(MinScaleProperty); }
            set { SetValue(MinScaleProperty, value); }
        }

        public static readonly DependencyProperty MaxScaleProperty =
            DependencyProperty.Register("MaxScale", typeof(double), typeof(ImageZoomBehavior),
                                        new PropertyMetadata(5.0));

        public double MaxScale
        {
            get { return (double)GetValue(MaxScaleProperty); }
            set { SetValue(MaxScaleProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.MouseWheel += OnMouseWheel;
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
            AssociatedObject.MouseMove += OnMouseMove;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= OnLoaded;
            AssociatedObject.MouseWheel -= OnMouseWheel;
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            AssociatedObject.MouseLeftButtonUp -= OnMouseLeftButtonUp;
            AssociatedObject.MouseMove -= OnMouseMove;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Инициализация трансформера для панорамирования и масштабирования
            _transformGroup = new TransformGroup();
            _translateTransform = new TranslateTransform();
            _scaleTransform = new ScaleTransform();
            _transformGroup.Children.Add(_scaleTransform);
            _transformGroup.Children.Add(_translateTransform);
            AssociatedObject.RenderTransform = _transformGroup;
            _imgSize = new Size(AssociatedObject.ActualWidth, AssociatedObject.ActualHeight);
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePosition = e.GetPosition(AssociatedObject);
            double zoomDelta = e.Delta > 0 ? 1.1 : 1 / 1.1;
            double newScale = _scaleTransform.ScaleX * zoomDelta;

            
            if (newScale >= MinScale && newScale <= MaxScale)
            {
                _scaleTransform.CenterX = mousePosition.X;
                _scaleTransform.CenterY = mousePosition.Y;
                _scaleTransform.ScaleX = newScale;
                _scaleTransform.ScaleY = newScale;
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _mouseDownPoint = e.GetPosition(AssociatedObject);
            _canMoveX = _canMoveY = false;
            AssociatedObject.CaptureMouse();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            AssociatedObject.ReleaseMouseCapture();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging) return;

            Point currentPoint = e.GetPosition(AssociatedObject);
            double dx = currentPoint.X - _mouseDownPoint.X;
            double dy = currentPoint.Y - _mouseDownPoint.Y;

            // Логика HandyControl по контролю пределов перемещения
            double marginX = _translateTransform.X;
            double marginY = _translateTransform.Y;

            if (_imgSize.Width * _scaleTransform.ScaleX > AssociatedObject.ActualWidth)
            {
                marginX += dx;
                if (marginX >= 0)
                    marginX = 0;
                else if (-marginX + AssociatedObject.ActualWidth >= _imgSize.Width * _scaleTransform.ScaleX)
                    marginX = AssociatedObject.ActualWidth - _imgSize.Width * _scaleTransform.ScaleX;
                _canMoveX = true;
            }

            if (_imgSize.Height * _scaleTransform.ScaleY > AssociatedObject.ActualHeight)
            {
                marginY += dy;
                if (marginY >= 0)
                    marginY = 0;
                else if (-marginY + AssociatedObject.ActualHeight >= _imgSize.Height * _scaleTransform.ScaleY)
                    marginY = AssociatedObject.ActualHeight - _imgSize.Height * _scaleTransform.ScaleY;
                _canMoveY = true;
            }

            _translateTransform.X = marginX;
            _translateTransform.Y = marginY;
        }
    }
}
