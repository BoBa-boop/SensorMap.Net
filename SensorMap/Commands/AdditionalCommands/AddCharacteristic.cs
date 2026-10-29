using SensorMap.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Commands.AdditionalCommands
{
    public class AddCharacteristic<T,T1> : IUndoRedoCommand
    {
        private readonly T inputObject;
        private readonly ObservableCollection<T1> collection;
        private readonly T1 newObject;
        private readonly bool currentModify;

        public AddCharacteristic(T1 _newObject, ObservableCollection<T1> _collection,T _inputObject,bool _currentModify)
        {
            collection = _collection;
            newObject = _newObject;
            currentModify = _currentModify;
            inputObject = _inputObject;
        }
        public void Do()
        {
            collection.Add(newObject);
            SetPropertyValue(true, "IsNew");
        }

        public void Undo()
        {
            collection.Remove(newObject);
            SetPropertyValue(currentModify, "IsNew");
        }
        private void SetPropertyValue(object value, string prop)
        {
            if (inputObject == null) return;

            var property = inputObject.GetType().GetProperty(prop);
            if (property != null && property.GetSetMethod() != null)
            {
                property.SetValue(inputObject, value);

            }
        }
    }
}
