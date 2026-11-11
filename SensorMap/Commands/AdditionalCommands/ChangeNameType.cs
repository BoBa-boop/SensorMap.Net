using SensorMap.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SensorMap.Commands.AdditionalCommands
{
    
    public class ChangeNameType<T> : IUndoRedoCommand
    {
        private readonly T _inputObject;
        private readonly string _propertyName;
        private readonly object _oldValue;
        private readonly bool _currentModify;
        private readonly object _newValue;
        public ChangeNameType(T inputObject,  object oldValue, object newValue, bool currentModify)
        {
            _inputObject = inputObject;
            _propertyName = "Name";
            _currentModify = currentModify;
            _oldValue = oldValue;
            _newValue = newValue;
        }
        public void Do()
        {
            SetPropertyValue(_newValue, _propertyName);
            SetPropertyValue(true, "IsNew");
        }

        public void Undo()
        {
            SetPropertyValue(_oldValue, _propertyName);
            SetPropertyValue(_currentModify, "IsNew");
        }
        private void SetPropertyValue(object value, string prop)
        {
            if (_inputObject == null) return;

            var property = _inputObject.GetType().GetProperty(prop);
            if (property != null && property.GetSetMethod() != null)
            {
                property.SetValue(_inputObject, value);

            }
        }
    }
}
