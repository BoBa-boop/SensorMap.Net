using SensorMap.CustomControls;
using SensorMap.Interfaces;
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
        private readonly ICollection<Rect> _old_rects;
        private readonly ICollection<Rect> _new_rects;
        private readonly Func<Point, Point> _worldToScreen;
        public TransformationSensors(IEnumerable<CustomSensor> elements, ICollection<Rect> new_rects,
                         Func<Point, Point> worldToScreen)
        {
            _elements = elements;
            _old_rects =
            {
                _old_rects.Add
                    ( 
                        new Rect
                        (
                            item.SensorData.X,
                            item.SensorData.Y, 
                            item.SensorData.Width,
                            item.SensorData.Height
                        )
                    );
            }
            _new_rects = new_rects;
            _worldToScreen = worldToScreen;
        }

        public void Do()
        {
            // Конвертируем мировые в экранные и устанавливаем
            //Point screenPos = _worldToScreen(_new_rect.Location);
            //Canvas.SetLeft(_element, screenPos.X);
            //Canvas.SetTop(_element, screenPos.Y);
            //_element.SensorData.X = screenPos.X;
            //_element.SensorData.Y = screenPos.Y;
            //_element.SensorData.Width = _new_rect.Width;
            //_element.SensorData.Height = _new_rect.Height;
            //_element.CustomBounds = new Rect(_new_rect.X, _new_rect.Y,
            //                             _new_rect.Width, _new_rect.Height);
        }

        public void Undo()
        {
            //// Конвертируем мировые в экранные и устанавливаем
            //Canvas.SetLeft(_element, _old_rect.X);
            //Canvas.SetTop(_element, _old_rect.Y);
            //_element.SensorData.X = _old_rect.X;
            //_element.SensorData.Y = _old_rect.Y;
            //_element.SensorData.Width = _old_rect.Width;
            //_element.SensorData.Height = _old_rect.Height;
            //_element.CustomBounds = new Rect(_old_rect.X, _old_rect.Y,
            //                             _old_rect.Width, _old_rect.Height);
        }

    }
}
