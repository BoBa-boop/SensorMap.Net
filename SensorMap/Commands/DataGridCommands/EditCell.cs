using ReactiveUI;
using SensorMap.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace SensorMap.Commands.DataGridCommands
{
    public class EditCell<T> : IUndoRedoCommand
    {
        private readonly T _inputObject;
        private readonly string _propertyName;
        private readonly object _oldValue;
        private readonly bool _currentModify;
        private readonly object _newValue;
        private readonly DataGrid _dataGrid;
        private readonly int _columnIndex;
        public EditCell(T inputObject, string propertyName, object oldValue, object newValue,
            DataGrid dataGrid, int columnIndex,bool currentModify)
        {
            _inputObject = inputObject;
            _propertyName = propertyName;
            _currentModify = currentModify;
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
            SetPropertyValue(_currentModify, "IsModified");
            FocusAndEditCell();
        }

        private void FocusAndEditCell()
        {
            if (_dataGrid == null || _inputObject == null || _columnIndex < 0 || _columnIndex >= _dataGrid.Columns.Count)
                return;
            _dataGrid.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                _dataGrid.SelectedItem = _inputObject;
                _dataGrid.ScrollIntoView(_inputObject);

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
