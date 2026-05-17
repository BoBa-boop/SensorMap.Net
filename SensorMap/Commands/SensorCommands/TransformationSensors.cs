using SensorMap.CustomControls;
using SensorMap.Interfaces;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Point = System.Windows.Point;

namespace SensorMap.Commands.SensorCommands
{
    public class TransformationSensors : IUndoRedoCommand
    {
        private readonly IEnumerable<CustomSensor> _elements;
        private readonly List<SensorAssignments> _old_elements = new List<SensorAssignments>();
        private readonly Func<Point, Point> _worldToScreen;
        public TransformationSensors(IEnumerable<SensorAssignments> sensorsData, IEnumerable<CustomSensor> elements,
                         Func<Point, Point> worldToScreen)
        {
            _elements = elements;
            _old_elements = sensorsData.Select(x => (SensorAssignments)x.Clone()).ToList();
            _worldToScreen = worldToScreen;
        }

        public void Do()
        {
            foreach (var item in _elements)
            {
                Point screenPos = item.CustomBounds.Location;
                Canvas.SetLeft(item, screenPos.X);
                Canvas.SetTop(item, screenPos.Y);
                item.SensorData.X = screenPos.X;
                item.SensorData.Y = screenPos.Y;
                
            }
            
        }

        public void Undo()
        {
            foreach (var item in _elements)
            {
                var obj = _old_elements.Where(x => x.Id == item.SensorData.Id).FirstOrDefault();
                item.SensorData.X = obj.X;
                item.SensorData.Y = obj.Y;
                Canvas.SetLeft(item, item.SensorData.X);
                Canvas.SetTop(item, item.SensorData.Y);
            }
            
        }

    }
}
