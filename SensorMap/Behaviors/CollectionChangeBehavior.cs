using Microsoft.Xaml.Behaviors;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Binding = System.Windows.Data.Binding;
using Button = System.Windows.Controls.Button;
using ListBox = System.Windows.Controls.ListBox;

namespace SensorMap.Behaviors
{
    public class CollectionChangeBehavior : Behavior<ListBox>
    {
        public static readonly DependencyProperty TargetButtonProperty =
        DependencyProperty.Register("TargetButton", typeof(Button), typeof(CollectionChangeBehavior));

        public Button TargetButton
        {
            get => (Button)GetValue(TargetButtonProperty);
            set => SetValue(TargetButtonProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(CollectionChangeBehavior),
                new PropertyMetadata(null, OnItemsSourceChanged));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            // Создаем Binding для отслеживания ItemsSource
            var binding = new Binding
            {
                Path = new PropertyPath("ItemsSource"),
                Source = AssociatedObject
            };
            BindingOperations.SetBinding(this, ItemsSourceProperty, binding);
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (CollectionChangeBehavior)d;

            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= behavior.OnCollectionChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += behavior.OnCollectionChanged;
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (TargetButton != null)
            {
                TargetButton.IsEnabled = true;
            }
        }

        protected override void OnDetaching()
        {
            if (ItemsSource is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged -= OnCollectionChanged;
            }
            base.OnDetaching();
        }
    }
}
