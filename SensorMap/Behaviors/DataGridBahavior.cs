using Microsoft.Xaml.Behaviors;
using SensorMap.Commands.DataGridCommands;
using SensorMap.Interfaces;
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
        private Window window;
        private EditCell<object> command;
        private Dictionary<string, object> originalFieldValues;


        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(DataGridBahavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            base.OnAttached();
            #region -UnSel
           
            #endregion
            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.BeginningEdit += OnBeginningEdit;
            AssociatedObject.CellEditEnding += OnCellEditEnding;
            AssociatedObject.RowEditEnding += AssociatedObject_RowEditEnding;
        }

        private void AssociatedObject_RowEditEnding(object? sender, DataGridRowEditEndingEventArgs e)
        {
            if (hasChangesBeenMade)
            {
                Command.Execute(command);
            }
            ResetStates();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            window = HandyControl.Controls.Window.GetWindow(AssociatedObject);
            if (window != null)
            {
                window.PreviewMouseDown += OnWindowPreviewMouseDown;
            }
        }

        private void OnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            var dataGrid = sender as DataGrid;
            if (e.Row.Item != null)
            {
                if (originalFieldValues != null)
                {
                    foreach (var kvp in originalFieldValues)
                    {
                        var currentValue = GetPropertyValue(e.Row.Item, kvp.Key);
                        int columnIndex = e.Column.DisplayIndex;
                        hasChangesBeenMade = !Equals(currentValue, kvp.Value);
                        if(hasChangesBeenMade)
                            command = new Commands.DataGridCommands.EditCell<object>
                                (e.Row.Item, kvp.Key, kvp.Value, currentValue, dataGrid, columnIndex);
                        break;
                    }

                    
                }
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
            AssociatedObject.Loaded -= OnLoaded;
            if (window != null) window.PreviewMouseDown -= OnWindowPreviewMouseDown;
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
                AssociatedObject.CancelEdit();
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
            var binding = e.Column.ClipboardContentBinding as System.Windows.Data.Binding;
            var propertyPath = binding?.Path?.Path.Split('.');

            if(propertyPath!=null)
            {
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
    
