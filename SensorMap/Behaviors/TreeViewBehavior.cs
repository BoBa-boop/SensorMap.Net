using Microsoft.Xaml.Behaviors;
using Newtonsoft.Json.Linq;
using SensorMap.Model;
using SensorMap.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SensorMap.Behaviors
{
    public class TreeViewBehavior:Behavior<TreeView>
    {
        public string SelectedItemType
        {
            get { return (string)GetValue(SelectedItemTypeProperty); }
            set { SetValue(SelectedItemTypeProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemTypeProperty =
            DependencyProperty.Register("SelectedItemType", typeof(string), typeof(TreeViewBehavior));

        public object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(TreeViewBehavior));
        
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.SelectedItemChanged += OnTreeViewSelectedItemChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.SelectedItemChanged -= OnTreeViewSelectedItemChanged;
            }
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if(e.NewValue!=null)
            {
            var type = e.NewValue.GetType();

            var valueProperty = type.GetProperty("Data");
                if (valueProperty != null && valueProperty.GetValue(e.NewValue) != null)
                {
                    this.SelectedItem = valueProperty.GetValue(e.NewValue);
                }
            }
        }
    }
}
