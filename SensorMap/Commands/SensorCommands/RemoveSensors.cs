using SensorMap.CustomControls;
using SensorMap.Interfaces;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SensorMap.Commands.SensorCommands
{
    public class RemoveSensors : IUndoRedoCommand
    {
        private readonly IEnumerable<CustomSensor> _elements;
        private readonly IEnumerable<SensorAssignments> _sensorsData;
        private readonly Canvas _canvas;
        private readonly ObservableCollection<SensorAssignments> _collection;
        public RemoveSensors(IEnumerable<SensorAssignments> sensors, IEnumerable<CustomSensor> sensorsVisual, Canvas canvas,
        ObservableCollection<SensorAssignments> collection)
        {
            _elements = sensorsVisual;
            _sensorsData = sensors;
            _canvas = canvas;
            _collection = collection;
        }
        public void Do()
        {
            foreach (var uiElement in _elements)
            {
                _canvas.Children.Remove(uiElement);
            }
            foreach (var item in _sensorsData)
            {
                _collection.Remove(item);
            }
        }

        public void Undo()
        {
            foreach (var uiElement in _elements)
            {
                _canvas.Children.Add(uiElement);
            }
            foreach (var item in _sensorsData)
            {
                _collection.Add(item);
            }
        }
    }
}
