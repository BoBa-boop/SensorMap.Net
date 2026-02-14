using Microsoft.Xaml.Behaviors;
using SensorMap.Model;
using SensorMap.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace SensorMap.Behaviors
{
    public class DataGridBahavior : Behavior<DataGrid>
    {
        private bool hasChangesBeenMade;
        private Dictionary<string, object> originalFieldValues;


        public object ViewModel
        {
            get { return (object)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(object), typeof(DataGridBahavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            #region -UnSel
            var window = Window.GetWindow(AssociatedObject);
            if (window != null)
            {
                window.PreviewMouseDown += OnWindowPreviewMouseDown;
            }
            #endregion
            
            AssociatedObject.BeginningEdit += OnBeginningEdit;
            AssociatedObject.CellEditEnding += OnCellEditEnding;
        }

        private void OnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Row.Item != null)
            {
                var prop = e.Row.Item!.GetType().GetProperty("IsModified");
                foreach (var kvp in originalFieldValues) неправильный выбор
                {
                    var currentValue = GetPropertyValue(e.Row.Item, kvp.Key);
                    hasChangesBeenMade = !Equals(currentValue, kvp.Value);
                    if (hasChangesBeenMade)
                    {
                        prop!.SetValue(e.Row.Item, true);

                        if (ViewModel is CRUD_VM vm)
                            vm.RecordEdit(e.Row.Item, kvp.Key, kvp.Value, currentValue);
                    }
                    break;
                }

                ResetStates();
            }
        }

        private void ResetStates()
        {
            hasChangesBeenMade = false;
            originalFieldValues.Clear();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            #region -UnSel
            var window = Window.GetWindow(AssociatedObject);
            if (window != null)
            {
                window.PreviewMouseDown -= OnWindowPreviewMouseDown;
            }
            #endregion
            AssociatedObject.BeginningEdit -= OnBeginningEdit;
            AssociatedObject.CellEditEnding -= OnCellEditEnding;
        }


        #region -UnSel
        private void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (AssociatedObject == null || !AssociatedObject.IsVisible) return;
            Point clickPosition = e.GetPosition(AssociatedObject);
            var hitTestResult = VisualTreeHelper.HitTest(AssociatedObject, e.GetPosition(AssociatedObject));
            if (hitTestResult == null)
            {
                AssociatedObject.UnselectAllCells();
                AssociatedObject.SelectedItem = null;
            }
            else
            {
                // Клик был внутри DataGrid - проверяем, был ли выбран какой-либо элемент
                CheckSelectionOnClick(hitTestResult, clickPosition);
            }
        }
        private void CheckSelectionOnClick(HitTestResult hitTestResult, Point clickPosition)
        {
            // Ищем DataGridRow или DataGridCell в визуальном дереве
            DependencyObject current = hitTestResult.VisualHit;
            bool foundDataGridElement = false;

            while (current != null && current != AssociatedObject)
            {
                if (current is DataGridRow || current is DataGridCell)
                {
                    foundDataGridElement = true;
                    break;
                }
                current = VisualTreeHelper.GetParent(current);
            }

            // Если клик был в области DataGrid, но не на строке или ячейке
            if (!foundDataGridElement)
            {
                AssociatedObject.UnselectAllCells();
                AssociatedObject.SelectedItem = null;
                
            }
        }
        #endregion
        private void OnBeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
        {
            if (e.Row.Item != null)
            {
                // Определяем, какая колонка редактируется
                GetEditColumnValue(e);
            }
        }
        #region Helpers
        private void GetEditColumnValue(DataGridBeginningEditEventArgs e)
        {
            if (e.EditingEventArgs.OriginalSource is TextBlock textBlock)
            {
                // Получаем свойство из Binding TextBlock
                var binding = textBlock.GetBindingExpression(TextBlock.TextProperty)?.ParentBinding as System.Windows.Data.Binding;
                //Так как некоторые свойства являются объектами, необходимо использовать первую часть привязки
                var propertyPath = binding?.Path.Path.Split('.');
                originalFieldValues = new Dictionary<string, object>
                {
                    [propertyPath[0]] = GetPropertyValue(e.Row.Item, propertyPath[0])
                };
            }
           
        }

        private object GetPropertyValue(object obj, string propertyPath)
        {
            var parts = propertyPath.Split('.');
            object current = obj;

            foreach (var part in parts)
            {
                if (current == null) return null;
                var prop = current.GetType().GetProperty(part);
                if (prop == null) return null;
                current = prop.GetValue(current);
            }

            return current;
        }
        #endregion
    }
}
    
