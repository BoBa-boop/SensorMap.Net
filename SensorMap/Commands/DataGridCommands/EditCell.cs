using SensorMap.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SensorMap.Commands.DataGridCommands
{
    public class EditCell<T> : IUndoRedoCommand
    {
        private readonly T _inputObject;
        private readonly string _propertyName;
        private readonly object _oldValue;
        private readonly object _newValue;
        private readonly DataGrid _dataGrid;
        private readonly int _columnIndex;
        public EditCell(T inputObject, string propertyName, object oldValue, object newValue,
            DataGrid dataGrid, int columnIndex)
        {
            _inputObject = inputObject;
            _propertyName = propertyName;
            _oldValue = oldValue;
            _newValue = newValue;
            _dataGrid = dataGrid;
            _columnIndex = columnIndex;
        }
        public void Do()
        {
            SetPropertyValue(_newValue, _propertyName);
            SetPropertyValue(true, "IsModified");
            FocusAndEditCell();
        }

        public void Undo()
        {
            SetPropertyValue(_oldValue, _propertyName);
            SetPropertyValue(false, "IsModified");
            FocusAndEditCell();
        }

        private void FocusAndEditCell()
        {
            if (_dataGrid == null || _inputObject == null || _columnIndex < 0 || _columnIndex >= _dataGrid.Columns.Count)
                return;

            // Откладываем выполнение, чтобы UI успел отреагировать на изменение данных
            _dataGrid.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                // 1. Делаем строку активной (Selected) и фокусируемся на ней
                _dataGrid.SelectedItem = _inputObject;
                _dataGrid.ScrollIntoView(_inputObject);

                // 2. Находим нужную колонку
                var column = _dataGrid.Columns[_columnIndex];

                // 3. Устанавливаем текущую ячейку
                _dataGrid.CurrentCell = new DataGridCellInfo(_inputObject, column);

                // 4. Даем команду DataGrid войти в режим редактирования ячейки
                _dataGrid.BeginEdit();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void SetPropertyValue(object value,string prop)
        {
            if(_inputObject==null) return;

            var property = _inputObject.GetType().GetProperty(prop);
            if (property != null&&property.GetSetMethod()!=null)
            {
                property.SetValue(_inputObject, value);
            }
        }
    }
}
