using SensorMap.Interfaces;
using SensorMap.Model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SensorMap.Commands.SensorCommands
{
    public class AddSensor : IUndoRedoCommand
    {
        private readonly UIElement _element;
        private readonly MapObject _mapData;
        private readonly Canvas _canvas;
        private readonly ObservableCollection<MapObject> _collection;
        public AddSensor(MapObject mapData, UIElement visualElement, Canvas canvas,
            ObservableCollection<MapObject> collection) 
        {
            _element = visualElement;
            _mapData = mapData;
            _canvas = canvas;
            _collection = collection;
        }
        public void Do()
        {
            _mapData.IsNew = true;
            _canvas.Children.Add(_element);
            if (!_collection.Contains(_mapData))
                _collection.Add(_mapData);
        }

        public void Undo()
        {
            _canvas.Children.Remove(_element);
            _collection.Remove(_mapData);
            _mapData.IsNew = false;
        }
    }
}
