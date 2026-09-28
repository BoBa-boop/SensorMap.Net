using Microsoft.Xaml.Behaviors;
using Newtonsoft.Json.Linq;
using SensorMap.Commands.DataGridCommands;
using SensorMap.Interfaces;
using SensorMap.Model;
using SensorMap.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using ComboBox = System.Windows.Controls.ComboBox;
using Point = System.Windows.Point;
using TextBox = System.Windows.Controls.TextBox;

namespace SensorMap.Behaviors
{
    public class DataGridBahavior : Behavior<DataGrid>
    {
        private bool hasChangesBeenMade;
        private Window window;
        private EditCell<object> command;
        private Dictionary<string, object> originalFieldValues;
        private ICollectionView collectionView;


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
                    var originalObject = originalFieldValues.First();
                    var objectAfterEdit = GetPropertyValue(e.Row.Item, originalObject.Key);
                    string originalProp = (GetPropertyValue(originalObject.Value, (string)originalFieldValues.Values.ElementAt(1))?? originalObject.Value).ToString();
                    string EditProp = (GetPropertyValue(objectAfterEdit, (string)originalFieldValues.Values.ElementAt(1))?? objectAfterEdit).ToString();
                    int columnIndex = e.Column.DisplayIndex;
                    hasChangesBeenMade = originalProp!=EditProp;
                    if (hasChangesBeenMade)
                    {
                        bool actualModify = (bool)originalFieldValues["Modify"];
                        command = new Commands.DataGridCommands.EditCell<object>
                            (e.Row.Item, originalObject.Key, originalObject.Value, objectAfterEdit, dataGrid, columnIndex, actualModify);
                        Command.Execute(command);
                    }
                    ResetStates();
                }
                collectionView.CollectionChanged -= CollectionView_CurrentChanged;
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
            var _dataGrid = AssociatedObject as DataGrid;
            if (e.Row.Item != null)
            {
                var dataGrid = AssociatedObject as DataGrid;
                collectionView = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
                GetEditColumnValue(e);
                collectionView.CollectionChanged += CollectionView_CurrentChanged;
            }
        }

        private void CollectionView_CurrentChanged(object? sender, EventArgs e)
        {
            var dataGrid = AssociatedObject as DataGrid;
            if (AssociatedObject.SelectedItem != null)
            {
                if (originalFieldValues != null&&originalFieldValues.Count()>0)
                {
                    var originalObject = originalFieldValues.First();
                    var objectAfterEdit = GetPropertyValue(AssociatedObject.SelectedItem, originalObject.Key);
                    string originalProp = (GetPropertyValue(originalObject.Value, (string)originalFieldValues.Values.ElementAt(1))).ToString();
                    string EditProp = (GetPropertyValue(objectAfterEdit, (string)originalFieldValues.Values.ElementAt(1))).ToString();
                    int columnIndex = AssociatedObject.CurrentColumn.DisplayIndex;
                    hasChangesBeenMade = originalProp != EditProp;
                    if (hasChangesBeenMade)
                    {
                        bool actualModify = (bool)originalFieldValues["Modify"];
                        command = new Commands.DataGridCommands.EditCell<object>
                            (AssociatedObject.SelectedItem, originalObject.Key, originalObject.Value, objectAfterEdit, dataGrid, columnIndex, actualModify);
                        Command.Execute(command);
                    }
                    ResetStates();
                }
                collectionView.CollectionChanged -= CollectionView_CurrentChanged;
            }
            

        }
        #region Helpers
        private void GetEditColumnValue(DataGridBeginningEditEventArgs e)
        {
            var binding = e.Column.ClipboardContentBinding as System.Windows.Data.Binding;
            var propertyPath = binding?.Path?.Path.Split('.');

            if(propertyPath!=null)
            {
                var editPropKey = propertyPath.Length > 1 ? propertyPath[1] : propertyPath[0];
                originalFieldValues = new Dictionary<string, object>
                {
                    [propertyPath[0]] = GetPropertyValue(e.Row.Item, propertyPath[0]),
                    ["EditProp"] = editPropKey,
                    ["Modify"] = GetPropertyValue(e.Row.Item, "IsModified")
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
    
