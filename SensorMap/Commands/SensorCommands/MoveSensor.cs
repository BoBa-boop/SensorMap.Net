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
    public class MoveSensor : IUndoRedoCommand
    {
        private readonly UIElement _element;
        private readonly SensorAssignments _sensor;
        private readonly Point _oldPointWorld;
        private readonly Point _newPointWorld;
        private readonly Func<Point, Point> _worldToScreen;
        public MoveSensor(UIElement element,Point newPointWorld, SensorAssignments sensor,
                         Func<Point, Point> worldToScreen)
        {
            _element = element;
            _sensor = sensor;
            _oldPointWorld.X = sensor.X;
            _oldPointWorld.Y = sensor.Y;
            _newPointWorld = newPointWorld;
            _worldToScreen = worldToScreen;
        }

        public void Do()
        {
            // Конвертируем мировые в экранные и устанавливаем
            Point screenPos = _worldToScreen(_newPointWorld);
            Canvas.SetLeft(_element, screenPos.X);
            Canvas.SetTop(_element, screenPos.Y);
            _sensor.X = screenPos.X;
            _sensor.Y = screenPos.Y;
        }

        public void Undo()
        {
            // Конвертируем мировые в экранные и устанавливаем
            
            Canvas.SetLeft(_element, _oldPointWorld.X);
            Canvas.SetTop(_element, _oldPointWorld.Y);
            _sensor.X = _oldPointWorld.X; 
            _sensor.Y = _oldPointWorld.Y;
        }
    }
}
