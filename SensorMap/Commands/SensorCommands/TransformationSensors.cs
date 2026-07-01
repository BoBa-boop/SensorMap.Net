using SensorMap.CustomControls;
using SensorMap.Interfaces;
using SensorMap.Model;
using System.Windows;
using System.Windows.Controls;
using Point = System.Windows.Point;

namespace SensorMap.Commands.SensorCommands
{
    public class TransformationSensors : IUndoRedoCommand
    {
        private readonly Canvas _canvas;
        private readonly List<SensorAssignments> _startValues = new();
        private readonly List<SensorAssignments> _updatedValues = new();
        private readonly Func<Point, Point> _worldToScreen;

        public TransformationSensors(IEnumerable<SensorAssignments> sensorsData, IEnumerable<CustomSensor> elements,
                         Func<Point, Point> worldToScreen)
        {
            _canvas = elements.FirstOrDefault()?.Parent as Canvas;
            _startValues = sensorsData.Select(x => (SensorAssignments)x.Clone()).ToList();
            _updatedValues = sensorsData.Select(x => (SensorAssignments)x.Clone()).ToList();
            foreach (var el in elements)
            {
                var uv = _updatedValues.FirstOrDefault(v => v.Id == el.SensorData.Id);
                if (uv != null)
                {
                    uv.X = el.CustomBounds.X;
                    uv.Y = el.CustomBounds.Y;
                    uv.Width = el.CustomBounds.Width;
                    uv.Height = el.CustomBounds.Height;
                }
            }
            _worldToScreen = worldToScreen;
        }

        private static CustomSensor? FindSensor(Canvas canvas, int sensorDataId)
        {
            return canvas.Children.OfType<CustomSensor>()
                .FirstOrDefault(s => s.SensorData.Id == sensorDataId);
        }

        public void Do()
        {
            foreach (var uv in _updatedValues)
            {
                var item = FindSensor(_canvas, uv.Id);
                if (item == null) continue;
                item.SensorData.X = uv.X;
                item.SensorData.Y = uv.Y;
                item.SensorData.Width = uv.Width;
                item.SensorData.Height = uv.Height;
                item.CustomBounds = new Rect(uv.X, uv.Y, uv.Width, uv.Height);
                Canvas.SetLeft(item, uv.X);
                Canvas.SetTop(item, uv.Y);
                item.SensorData.IsModified = true;
            }
        }

        public void Undo()
        {
            foreach (var sv in _startValues)
            {
                var item = FindSensor(_canvas, sv.Id);
                if (item == null) continue;
                item.SensorData.X = sv.X;
                item.SensorData.Y = sv.Y;
                item.SensorData.Width = sv.Width;
                item.SensorData.Height = sv.Height;
                item.CustomBounds = new Rect(sv.X, sv.Y, sv.Width, sv.Height);
                Canvas.SetLeft(item, sv.X);
                Canvas.SetTop(item, sv.Y);
            }
        }
    }
}
