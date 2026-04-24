using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Button = System.Windows.Controls.Button;
using ListBox = System.Windows.Controls.ListBox;
using Point = System.Windows.Point;
using TextBox = System.Windows.Controls.TextBox;

namespace SensorMap.Behaviors
{
    public class ListBoxBehavior : Behavior<ListBox>
    {


        public bool CanUnselectItem
        {
            get { return (bool)GetValue(CanUnselectItemProperty); }
            set { SetValue(CanUnselectItemProperty, value); }
        }

        public static readonly DependencyProperty CanUnselectItemProperty =
            DependencyProperty.Register("CanUnselectItem", typeof(bool), typeof(ListBoxBehavior), new PropertyMetadata(false));

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.PreviewMouseDown += OnWindowPreviewMouseDown;
        }

        private void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject == null || !AssociatedObject.IsVisible || !CanUnselectItem) return;
            Point clickPosition = e.GetPosition(AssociatedObject);
            var hitTestResult = VisualTreeHelper.HitTest(AssociatedObject, e.GetPosition(AssociatedObject));
            if (hitTestResult == null)
            {
                AssociatedObject.SelectedItem = null;
            }
            else
            {
                CheckSelectionOnClick(hitTestResult, clickPosition);
            }
        }

        private void CheckSelectionOnClick(HitTestResult hitTestResult, Point clickPosition)
        {
            DependencyObject current = hitTestResult.VisualHit;
            bool isItem = false;
            while (current != null && current != AssociatedObject)
            {
                if (current is ListBoxItem)
                {
                    isItem = true;
                    break;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            if (!isItem)
            {
                AssociatedObject.SelectedItem = null;
            }
        }
    }
}
