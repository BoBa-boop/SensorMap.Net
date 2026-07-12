using DynamicData;
using SensorMap.CustomControls;
using SensorMap.Interfaces;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace SensorMap.Commands.SensorCommands
{
    public class AddSensor : IUndoRedoCommand
    {
        private readonly FrameworkElement _element;
        private readonly MapObject _mapData;
        private readonly Canvas _canvas;
        private readonly ObservableCollection<MapObject> _collection;
        public AddSensor(MapObject mapData, FrameworkElement sensorVisual,Canvas canvas,
        ObservableCollection<MapObject> collection) 
        {
            _element = sensorVisual; //(FrameworkElement)sensorVisual.MapData.Clone();//был clone
            _mapData = mapData;
            _canvas = canvas;
            _collection = collection;
        }
        public void Do()
        {
            _mapData.IsNew = true;
            _canvas.Children.Add(_element);
            Canvas.SetLeft(_element, _mapData.X);
            Canvas.SetTop(_element, _mapData.Y);
            if (!_collection.Contains(_mapData))
                _collection.Add(_mapData);
            
        }

        public void Undo()
        {
            var item = _mapData.FindInCanvas(_canvas);
            if (item == null) return;
            _canvas.Children.Remove(item);
            _collection.Remove(_mapData);
            _mapData.IsNew = false;
        }
    }
}
