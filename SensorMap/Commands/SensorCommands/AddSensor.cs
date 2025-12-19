using DynamicData;
using SensorMap.Interfaces;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace SensorMap.Commands.SensorCommands
{
    public class AddSensor : ICommandSensors
    {
        private readonly Ellipse _element;
        private readonly SensorAssignments _sensorData;
        private readonly Canvas _canvas;
        private readonly ObservableCollection<SensorAssignments> _collection;
        public AddSensor(SensorAssignments sensor, Ellipse sensorVisual,Canvas canvas,
        ObservableCollection<SensorAssignments> collection) 
        {
            _element = sensorVisual;
            _sensorData = sensor;
            _canvas = canvas;
            _collection = collection;
        }
        public void Do()
        {
            if (!_canvas.Children.Contains(_element))
            {
                _canvas.Children.Add(_element);
            }
            if (!_collection.Contains(_sensorData))
            {
                _collection.Add(_sensorData); 
            }
        }

        public void Undo()
        {
            _canvas.Children.Remove(_element);
            _collection.Remove(_sensorData);
        }
    }
}
