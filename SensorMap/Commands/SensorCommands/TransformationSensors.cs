using SensorMap.CustomControls;
using SensorMap.Interfaces;
using SensorMap.Model;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using Point = System.Windows.Point;

namespace SensorMap.Commands.SensorCommands
{
    public class TransformationSensors : IUndoRedoCommand
    {
        private readonly IEnumerable<CustomSensor> _elements;
        /// <summary>
        /// Изначальные значения
        /// </summary>
        private readonly List<SensorAssignments> _startValues = new List<SensorAssignments>();
        /// <summary>
        /// Обновленные значения
        /// </summary>
        private readonly List<CustomSensor> _updatedValues = new List<CustomSensor>();
        private readonly Func<Point, Point> _worldToScreen;
        public TransformationSensors(IEnumerable<SensorAssignments> sensorsData, IEnumerable<CustomSensor> elements,
                         Func<Point, Point> worldToScreen)
        {
            _elements = elements;
            _startValues = sensorsData.Select(x => (SensorAssignments)x.Clone()).ToList();
            _updatedValues = elements.Select(x => (CustomSensor)x.Clone()).ToList();
            _worldToScreen = worldToScreen;
        }

        public void Do()
        {
            foreach (var item in _elements)
            {
                var obj = _updatedValues.Where(x=>x.SensorData.Id==item.SensorData.Id).First();
                Point screenPos = obj.CustomBounds.Location;
                Canvas.SetLeft(item, screenPos.X);
                Canvas.SetTop(item, screenPos.Y);
                item.SensorData.X = screenPos.X;
                item.SensorData.Y = screenPos.Y;
                item.SensorData.Width = obj.CustomBounds.Width;
                item.SensorData.Height = obj.CustomBounds.Height;
                item.CustomBounds = new Rect(obj.CustomBounds.X, obj.CustomBounds.Y,
                                             obj.CustomBounds.Width, obj.CustomBounds.Height);
            }
            
        }

        public void Undo()
        {
            foreach (var item in _elements)
            {
                var obj = _startValues.Where(x => x.Id == item.SensorData.Id).FirstOrDefault();
                item.SensorData.X = obj.X;
                item.SensorData.Y = obj.Y;
                Canvas.SetLeft(item, item.SensorData.X);
                Canvas.SetTop(item, item.SensorData.Y);
                item.SensorData.Width = obj.Width;
                item.SensorData.Height = obj.Height;
                item.CustomBounds = new Rect(obj.X, obj.Y,
                                             obj.Width, obj.Height);
            }
            
        }

    }
}
