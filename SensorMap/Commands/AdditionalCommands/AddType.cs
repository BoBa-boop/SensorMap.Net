using SensorMap.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Commands.AdditionalCommands
{
    
    public class AddType<T> : IUndoRedoCommand
    {
        private readonly ObservableCollection<T> collection;
        private readonly T newObject;

        public AddType(T _newObject, ObservableCollection<T> _collection)
        {
            collection= _collection;
            newObject = _newObject;
        }
        public void Do()
        {
            collection.Add(newObject);
        }

        public void Undo()
        {
            collection.Remove(newObject);
        }
    }
}
