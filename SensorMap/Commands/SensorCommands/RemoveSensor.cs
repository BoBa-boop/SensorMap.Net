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
            _elements = sensorsVisual.Select(x => (CustomSensor)x.Clone()).ToList();
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
            foreach (var sensor in _elements)
            {
                var item = FindSensor(_canvas, sensor.SensorData.Id);
                if (item is null) return;
                item.SensorData.ToDelete=true;
                _canvas.Children.Remove(item);
                //if (_collection.Contains(item.SensorData))
                //{
                //    _collection.Remove(item.SensorData);
                //}
            }

        }

        public void Undo()
        {
            foreach (var sensor in _elements.Where(x=>x.SensorData.ToDelete==true))
            {
                sensor.SensorData.ToDelete = false;
                _canvas.Children.Add(sensor);
                Canvas.SetLeft(sensor, sensor.SensorData.X);
                Canvas.SetTop(sensor, sensor.SensorData.Y);
                //_collection.Add(item.SensorData);
            }

        }
    }
}
