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
using System.Windows.Threading;
using Brushes = System.Windows.Media.Brushes;
using ComboBox = System.Windows.Controls.ComboBox;
using Point = System.Windows.Point;
using TextBox = System.Windows.Controls.TextBox;

namespace SensorMap.Behaviors
{
    
    public class DataGridBahavior : Behavior<DataGrid>
    {
        private bool hasChangesBeenMade;
        private Window? window;
        private EditCell<object>? command;
        private Dictionary<string, object>? originalFieldValues;


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

            AssociatedObject.Loaded += OnLoaded;
            AssociatedObject.BeginningEdit += OnBeginningEdit;
            AssociatedObject.CellEditEnding += OnCellEditEnding;
            AssociatedObject.CommandBindings.Add(new CommandBinding(GroupCommands.ExpandAll,(_,_)=>SetAll(true)));
            AssociatedObject.CommandBindings.Add(new CommandBinding(GroupCommands.CollapseAll, (_, _) => SetAll(false)));
        }

        private void SetAll(bool expand)
        {
            var grid = AssociatedObject;
            GroupExpansion.GetState(grid).Reset(expand);
            grid.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (var exp in FindChildren<Expander>(grid).Where(GroupExpansion.GetTrack).ToList())
                {
                    exp.IsExpanded = expand;
                }
            }), DispatcherPriority.Loaded);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (window != null)
                window.PreviewMouseDown -= OnWindowPreviewMouseDown;
            window = HandyControl.Controls.Window.GetWindow(AssociatedObject);
            if (window != null)
            {
                window.PreviewMouseDown += OnWindowPreviewMouseDown;
            }
        }

        private void OnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.Row.Item is not { } rowItem) return;
            if (sender is not DataGrid dataGrid) return;
            if (originalFieldValues == null || originalFieldValues.Count==0) return;

            try
            {
                // 1. Достаём реальный контрол редактирования из ContentPresenter (для TemplateColumn)
                var editingRoot = e.EditingElement;
                var comboBox = FindChild<ComboBox>(editingRoot);
                var textBox = FindChild<TextBox>(editingRoot);

                // 2. Определяем новое значение
                object? objectAfterEdit;
                if (comboBox != null)
                    objectAfterEdit = comboBox.SelectedItem;      // объект DeviceType
                else if (textBox != null)
                    objectAfterEdit = textBox.Text;               // строка (Name)
                else
                    objectAfterEdit = GetPropertyValue(rowItem, GetOriginalKey()); // fallback

                // 3. Явно берём исходное значение по ключу, а не First()
                string originalKey = GetOriginalKey();               // "DeviceType" / "Name" / ...
                object? originalObjValue = originalFieldValues.TryGetValue(originalKey, out var ov) ? ov : null;

                // 4. Ключ вложенного свойства ("Name")
                string subProp = originalFieldValues.TryGetValue("EditProp", out var ep) && ep is string s
                    ? s
                    : originalKey;

                // 5. Сравниваем "плоские" значения (для вложенных путей — по под-свойству)
                string originalProp = ExtractComparable(originalObjValue, subProp);
                string editProp = ExtractComparable(objectAfterEdit, subProp);

                if (string.IsNullOrEmpty(originalProp) && string.IsNullOrEmpty(editProp))
                    return;

                bool hasChanges = !string.Equals(originalProp, editProp, StringComparison.Ordinal);
                if (!hasChanges) return;

                // 6. Безопасно читаем Modify
                bool actualModify = originalFieldValues.TryGetValue("Modify", out var m) && m is bool b && b;

                int columnIndex = e.Column.DisplayIndex;

                command = new EditCell<object>(
                    rowItem,
                    originalKey,
                    originalObjValue!,       // исходное значение (объект DeviceType или строка Name)
                    objectAfterEdit!,        // новое (объект DeviceType из ComboBox или строка из TextBox)
                    dataGrid,
                    columnIndex,
                    actualModify);

                Command.Execute(command);
            }
            finally
            {
                ResetStates();
            }
        }

        private void ResetStates()
        {
            hasChangesBeenMade = false;
            if(originalFieldValues!=null) originalFieldValues.Clear();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            #region -UnSel
            var window = Window.GetWindow(AssociatedObject);
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
            var hitTestResult = VisualTreeHelper.HitTest(AssociatedObject, clickPosition);
             if (Keyboard.FocusedElement is ComboBox { IsDropDownOpen: true }) return;
            if (IsInsideComboBoxPopup(e.OriginalSource as DependencyObject)) return;
            if (hitTestResult == null)
            {
               if (Keyboard.FocusedElement is DependencyObject fe && GetParent(fe) != null) return;
                //Клик вне DG
                EndEdit();
                AssociatedObject.UnselectAllCells();
                AssociatedObject.SelectedItem = null;
            }
            else
            {
                CheckSelectionOnClick(hitTestResult);
            }
        }
        private bool IsInsideComboBoxPopup(DependencyObject? source)
        {
            while(source != null)
            {
                if (source is ComboBoxItem) return true;
                source = GetParent(source);
            }
            return false;
        }
        private void CheckSelectionOnClick(HitTestResult hitTestResult)
        {
            DependencyObject? current = hitTestResult.VisualHit;
            DataGridCell? clickedCell = null;
            bool foundDataGridElement = false;

            while (current != null && current != AssociatedObject)
            {
                if (current is DataGridCell cell)
                {
                    clickedCell = cell;
                    foundDataGridElement = true;
                    break;
                }
                if (current is DataGridRow)
                {
                    foundDataGridElement = true;
                    break;
                }
                current = GetParent(current);
            }
            if (clickedCell != null && clickedCell.IsEditing) return;

            

            // Если клик был в области DataGrid, но не на строке или ячейке
            if (!foundDataGridElement)
            {
                AssociatedObject.UnselectAllCells();
                AssociatedObject.SelectedItem = null;
                EndEdit();
            }
        }
        private void EndEdit()
        {
            AssociatedObject.CommitEdit(DataGridEditingUnit.Cell, true);
            //AssociatedObject.CommitEdit(DataGridEditingUnit.Row, true);
        }
        #endregion
        private void OnBeginningEdit(object? sender, DataGridBeginningEditEventArgs e)
        {
            var _dataGrid = AssociatedObject as DataGrid;
            if (e.Row.Item != null)
            {
                GetEditColumnValue(e);
            }
        }

        #region Helpers
       
        private string GetOriginalKey()
        {
            // служебные ключи, которые не являются именем свойства
            var serviceKeys = new HashSet<string> { "EditProp", "Modify" };
            return originalFieldValues!.Keys.First(k => !serviceKeys.Contains(k));
        }

        // Достаём сравнимую строку: если значение — объект, берём его под-свойство (subProp);
        // если subProp == имени ключа (плоское свойство) — ToString() самого значения.
        private static string ExtractComparable(object? value, string subProp)
        {
            if (value == null) return string.Empty;

            // Если subProp совпадает с именем ключа (например, "Name") — это плоское свойство
            var type = value.GetType();
            var prop = type.GetProperty(subProp);
            if (prop != null && prop.DeclaringType == type)
            {
                var inner = prop.GetValue(value);
                return inner?.ToString() ?? string.Empty;
            }

            // Иначе — это сам объект (DeviceType), и subProp — его под-свойство ("Name")
            return value.ToString() ?? string.Empty;
        }
        private void GetEditColumnValue(DataGridBeginningEditEventArgs e)
        {
            var binding = e.Column.ClipboardContentBinding as System.Windows.Data.Binding;
            string[]? propertyPath = binding?.Path?.Path.Split('.');

            if(propertyPath!=null && e.Row.Item!=null)
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
            object? current = obj;

            foreach (var part in parts)
            {
                if (current == null) return null!;
                var prop = current.GetType().GetProperty(part);
                if (prop == null) return null!;
                current = prop.GetValue(current);
            }

            return current!;
        }
        private static IEnumerable<T> FindChildren<T>(DependencyObject parent)
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) yield return t;
                foreach (var nested in FindChildren<T>(child))
                {
                    yield return nested;
                }
            }
        }
        private static T? FindChild<T>(DependencyObject? parent) where T : DependencyObject
        {
            if (parent == null) return null;
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var nested = FindChild<T>(child);
                if (nested != null) return nested;
            }
            return null;
        }
        private static DependencyObject? GetParent(DependencyObject child)
        {
            if(child is Visual)
                return VisualTreeHelper.GetParent(child);
            return LogicalTreeHelper.GetParent(child);
        }
        #endregion
    }
}
    
