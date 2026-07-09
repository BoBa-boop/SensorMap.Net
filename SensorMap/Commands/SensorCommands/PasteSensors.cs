using SensorMap.Interfaces;
using SensorMap.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SensorMap.Commands.SensorCommands
{
    public class PasteSensors : IUndoRedoCommand
    {
        private readonly IEnumerable<UIElement> _elements;
        private readonly IEnumerable<MapObject> _mapData;
        private readonly Canvas _canvas;
        private readonly ObservableCollection<MapObject> _collection;
        public PasteSensors(IEnumerable<MapObject> mapData, IEnumerable<UIElement> elements, Canvas canvas,
            ObservableCollection<MapObject> collection)
        {
            _elements = elements;
            _mapData = mapData;
            _canvas = canvas;
            _collection = collection;
        }
        public void Do()
        {
            foreach (var uiElement in _elements)
            {
                _canvas.Children.Add(uiElement);
            }
            foreach (var item in _mapData)
            {
                item.IsNew = true;
                _collection.Add(item);
            }
        }

        public void Undo()
        {
            foreach (var uiElement in _elements)
            {
                _canvas.Children.Remove(uiElement);
            }
            foreach (var item in _mapData)
            {
                item.IsNew = false;
                _collection.Remove(item);
            }
        }
    }
}
