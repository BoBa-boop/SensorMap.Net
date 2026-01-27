using System.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Xaml.Behaviors;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using Point = System.Windows.Point;

namespace SensorMap.Behaviors
{
    public class DataGridBahavior : Behavior<DataGrid>
    {

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
        
    }
}
    
