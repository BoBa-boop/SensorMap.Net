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
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace SensorMap.Commands.SensorCommands
{
    public class TransformationSensor : IUndoRedoCommand
    {
        private readonly CustomSensor _element;
        private readonly Rect _old_rect;
        private readonly Rect _new_rect;
        private readonly Func<Point, Point> _worldToScreen;
        public TransformationSensor(CustomSensor element, Rect new_rect,
                         Func<Point, Point> worldToScreen)
        {
            _element = element;
            _old_rect = new Rect(_element.SensorData.X,_element.SensorData.Y,_element.SensorData.Width, _element.SensorData.Height);
            _new_rect = new_rect;
            _worldToScreen = worldToScreen;
        }

        public void Do()
        {
            // Конвертируем мировые в экранные и устанавливаем
            
            Point screenPos = _worldToScreen(_new_rect.Location);
            Canvas.SetLeft(_element, screenPos.X);
            Canvas.SetTop(_element, screenPos.Y);
            _element.SensorData.X = screenPos.X;
            _element.SensorData.Y = screenPos.Y;
            _element.SensorData.Width = _new_rect.Width;
            _element.SensorData.Height = _new_rect.Height;
            _element.CustomBounds = new Rect(_new_rect.X, _new_rect.Y,
                                         _new_rect.Width, _new_rect.Height);
            _element.SensorData.IsModified = true;
        }

        public void Undo()
        {
            // Конвертируем мировые в экранные и устанавливаем
            Canvas.SetLeft(_element, _old_rect.X);
            Canvas.SetTop(_element, _old_rect.Y);
            _element.SensorData.X = _old_rect.X;
            _element.SensorData.Y = _old_rect.Y;
            _element.SensorData.Width = _old_rect.Width;
            _element.SensorData.Height = _old_rect.Height;
            _element.CustomBounds = new Rect(_old_rect.X, _old_rect.Y,
                                         _old_rect.Width, _old_rect.Height);
        }
        
    }
}
