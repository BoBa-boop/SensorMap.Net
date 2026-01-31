using Microsoft.Xaml.Behaviors;
using SensorMap.Model;
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
        private object startStateItem;

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
                if (!e.Row.Item.Equals(startStateItem)) hasChangesBeenMade = true;
                // Проверяем, изменилось ли состояние сущности после завершения редактирования
                var type = e.Row.Item.GetType();
                var valueProperty = type.GetProperty("IsModified");
                object currentValue = valueProperty?.GetValue(e.Row.Item)!;
                if ((bool?)currentValue == true && !hasChangesBeenMade)
                {
                    // Если изменений не произошло, сбрасываем флаг обратно
                    valueProperty.SetValue(e.Row.Item, false);
                }

                // Сбрасываем флаг для следующей проверки
                hasChangesBeenMade = false;
            }
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
                startStateItem = new { e.Row.Item };
                var type = e.Row.Item.GetType();
                var valueProperty = type.GetProperty("IsModified");
                object currentValue = valueProperty?.GetValue(e.Row.Item)!;
                
                if ((bool?)currentValue == false)
                    valueProperty.SetValue(e.Row.Item, true);
            }
        }
    }
}
    
