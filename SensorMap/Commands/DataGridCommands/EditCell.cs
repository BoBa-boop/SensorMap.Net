using SensorMap.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Commands.DataGridCommands
{
    public class EditCell<T> : IUndoRedoCommand
    {
        private readonly T _inputObject;
        private readonly string _propertyName;
        private readonly object _oldValue;
        private readonly object _newValue;

        public EditCell(T inputObject, string propertyName, object oldValue, object newValue)
        {
            _inputObject = inputObject;
            _propertyName = propertyName;
            _oldValue = oldValue;
            _newValue = newValue;
        }
        public void Do()
        {
            SetPropertyValue(_newValue, _propertyName);
        }

        public void Undo()
        {
            SetPropertyValue(_oldValue, _propertyName);
            SetPropertyValue(true, "IsModified");
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
