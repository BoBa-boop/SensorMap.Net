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
    public class RemoveSensor : IUndoRedoCommand
    {
        private readonly List<CustomSensor> _elements;
        private readonly Canvas _canvas;
        private readonly ObservableCollection<SensorAssignments> _collection;
        public RemoveSensor(List<CustomSensor> sensorsVisual, Canvas canvas,
        ObservableCollection<SensorAssignments> collection)
        {
            _elements = sensorsVisual;
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
            foreach (var item in _elements)
            {
                item.SensorData.ToDelete=true;
                if (_canvas.Children.Contains(item))
                {
                    _canvas.Children.Remove(item);
                }
                //if (_collection.Contains(item.SensorData))
                //{
                //    _collection.Remove(item.SensorData);
                //}
            }

        }

        public void Undo()
        {
            foreach (var sensor in _elements)
            {
                var item = FindSensor(_canvas, sensor.SensorData.Id);
                if (item == null) return;
                item.SensorData.ToDelete = false;
                _canvas.Children.Add(item);
                //_collection.Add(item.SensorData);
            }

        }
    }
}
