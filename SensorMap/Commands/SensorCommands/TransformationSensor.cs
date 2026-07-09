using SensorMap.Interfaces;
using SensorMap.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using Point = System.Windows.Point;

namespace SensorMap.Commands.SensorCommands
{
    public class TransformationSensor : IUndoRedoCommand
    {
        private readonly UIElement _element;
        private readonly MapObject _mapData;
        private readonly Rect _old_rect;
        private readonly Rect _new_rect;
        private readonly Func<Point, Point> _worldToScreen;
        public TransformationSensor(UIElement element, MapObject mapData, Rect new_rect,
                         Func<Point, Point> worldToScreen)
        {
            _element = element;
            _mapData = mapData;
            _old_rect = new Rect(_mapData.X, _mapData.Y, _mapData.Width, _mapData.Height);
            _new_rect = new_rect;
            _worldToScreen = worldToScreen;
        }

        public void Do()
        {
            Point screenPos = _worldToScreen(_new_rect.Location);
            Canvas.SetLeft(_element, screenPos.X);
            Canvas.SetTop(_element, screenPos.Y);
            _mapData.X = screenPos.X;
            _mapData.Y = screenPos.Y;
            _mapData.Width = _new_rect.Width;
            _mapData.Height = _new_rect.Height;
            if (_element is IMapElement mapElement)
            {
                mapElement.SetCustomBounds(new Rect(_new_rect.X, _new_rect.Y,
                                             _new_rect.Width, _new_rect.Height));
            }
            _mapData.IsModified = true;
        }

        public void Undo()
        {
            Canvas.SetLeft(_element, _old_rect.X);
            Canvas.SetTop(_element, _old_rect.Y);
            _mapData.X = _old_rect.X;
            _mapData.Y = _old_rect.Y;
            _mapData.Width = _old_rect.Width;
            _mapData.Height = _old_rect.Height;
            if (_element is IMapElement mapElement)
            {
                mapElement.SetCustomBounds(new Rect(_old_rect.X, _old_rect.Y,
                                             _old_rect.Width, _old_rect.Height));
            }
        }
    }
}
