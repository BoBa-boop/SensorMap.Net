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
using System.Windows.Controls;
using System.Windows.Shapes;

namespace SensorMap.Commands.SensorCommands
{
    public class AddSensor : IUndoRedoCommand
    {
        private readonly CustomSensor _element;
        private readonly SensorAssignments _sensorData;
        private readonly Canvas _canvas;
        private readonly ObservableCollection<SensorAssignments> _collection;
        public AddSensor(SensorAssignments sensor, CustomSensor sensorVisual,Canvas canvas,
        ObservableCollection<SensorAssignments> collection) 
        {
            _element = sensorVisual;
            _sensorData = sensor;
            _canvas = canvas;
            _collection = collection;
        }
        private static CustomSensor? FindSensor(Canvas canvas, int sensorDataId)
        {
            return canvas.Children.OfType<CustomSensor>()
                .FirstOrDefault(s => s.SensorData.Id == sensorDataId);
        }
        public void Do()
        {
            _sensorData.IsNew = true;
            _canvas.Children.Add(_element);
            if (!_collection.Contains(_sensorData))
                _collection.Add(_sensorData);
            
        }

        public void Undo()
        {
            var item = FindSensor(_canvas, _sensorData.Id);
            if (item == null) return;
            _canvas.Children.Remove(item);
            _collection.Remove(_sensorData);
            _sensorData.IsNew = false;
        }
    }
}
