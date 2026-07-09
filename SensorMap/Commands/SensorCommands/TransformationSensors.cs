using SensorMap.Interfaces;
using SensorMap.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Point = System.Windows.Point;

namespace SensorMap.Commands.SensorCommands
{
    public class TransformationSensors : IUndoRedoCommand
    {
        private readonly Canvas _canvas;
        private readonly List<MapObject> _startValues = new();
        private readonly List<MapObject> _updatedValues = new();
        private readonly Func<Point, Point> _worldToScreen;

        public TransformationSensors(IEnumerable<MapObject> mapData, IEnumerable<UIElement> elements,
                         Func<Point, Point> worldToScreen)
        {
            _canvas = (elements.FirstOrDefault() as FrameworkElement)?.Parent as Canvas;
            _startValues = mapData.Select(x => (MapObject)x.Clone()).ToList();
            _updatedValues = mapData.Select(x => (MapObject)x.Clone()).ToList();
            foreach (var el in elements)
            {
                if (el is IMapElement mapElement)
                {
                    var uv = _updatedValues.FirstOrDefault(v => v.Id == mapElement.MapData.Id);
                    if (uv != null)
                    {
                        var bounds = mapElement.GetCustomBounds();
                        uv.X = bounds.X;
                        uv.Y = bounds.Y;
                        uv.Width = bounds.Width;
                        uv.Height = bounds.Height;
                    }
                }
            }
            _worldToScreen = worldToScreen;
        }

        public void Do()
        {
            foreach (var uv in _updatedValues)
            {
                var item = _canvas.Children.OfType<UIElement>()
                    .FirstOrDefault(e => e is IMapElement me && me.MapData.Id == uv.Id);
                if (item == null) continue;
                if (item is IMapElement mapElement)
                {
                    mapElement.MapData.X = uv.X;
                    mapElement.MapData.Y = uv.Y;
                    mapElement.MapData.Width = uv.Width;
                    mapElement.MapData.Height = uv.Height;
                    mapElement.SetCustomBounds(new Rect(uv.X, uv.Y, uv.Width, uv.Height));
                }
                Canvas.SetLeft(item, uv.X);
                Canvas.SetTop(item, uv.Y);
                uv.IsModified = true;
            }
        }

        public void Undo()
        {
            foreach (var sv in _startValues)
            {
                var item = _canvas.Children.OfType<UIElement>()
                    .FirstOrDefault(e => e is IMapElement me && me.MapData.Id == sv.Id);
                if (item == null) continue;
                if (item is IMapElement mapElement)
                {
                    mapElement.MapData.X = sv.X;
                    mapElement.MapData.Y = sv.Y;
                    mapElement.MapData.Width = sv.Width;
                    mapElement.MapData.Height = sv.Height;
                    mapElement.SetCustomBounds(new Rect(sv.X, sv.Y, sv.Width, sv.Height));
                }
                Canvas.SetLeft(item, sv.X);
                Canvas.SetTop(item, sv.Y);
            }
        }
    }
}
