using SensorMap.Interfaces;
using SensorMap.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SensorMap.Commands.SensorCommands
{
    public class RemoveSensor : IUndoRedoCommand
    {
        private readonly List<UIElement> _elements;
        private readonly Canvas _canvas;
        private readonly ObservableCollection<MapObject> _collection;
        public RemoveSensor(List<UIElement> elements, Canvas canvas,
            ObservableCollection<MapObject> collection)
        {
            _elements = elements;
            _canvas = canvas;
            _collection = collection;
        }
        public void Do()
        {
            foreach (var item in _elements)
            {
                if (item is IMapElement mapElement)
                {
                    mapElement.MapData.ToDelete = true;
                }
                if (_canvas.Children.Contains(item))
                {
                    _canvas.Children.Remove(item);
                }
            }
        }

        public void Undo()
        {
            foreach (var item in _elements)
            {
                if (item is IMapElement mapElement)
                {
                    mapElement.MapData.ToDelete = false;
                }
                _canvas.Children.Add(item);
            }
        }
    }
}
