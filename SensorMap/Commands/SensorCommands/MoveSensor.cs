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

namespace SensorMap.Commands.SensorCommands
{
    public class MoveSensor : ICommandSensors
    {
        private readonly UIElement _element;
        private readonly SensorAssignments _sensor;
        private readonly double _oldWorldX;
        private readonly double _oldWorldY;
        private readonly double _newWorldX;
        private readonly double _newWorldY;
        private readonly Func<double, double, Point> _worldToScreen;

        public MoveSensor(UIElement element,
                         double newWorldX, double newWorldY, SensorAssignments sensor,
                         Func<double, double, Point> worldToScreen)
        {
            _element = element;
            _sensor = sensor;
            _oldWorldX = sensor.X;
            _oldWorldY = sensor.Y;
            _newWorldX = newWorldX;
            _newWorldY = newWorldY;
            _worldToScreen = worldToScreen;
        }

        public void Do()
        {
            // Конвертируем мировые в экранные и устанавливаем
            Point screenPos = _worldToScreen(_newWorldX, _newWorldY);
            Canvas.SetLeft(_element, screenPos.X);
            Canvas.SetTop(_element, screenPos.Y);
            _sensor.X = screenPos.X;
            _sensor.Y = screenPos.Y;
        }

        public void Undo()
        {
            // Конвертируем мировые в экранные и устанавливаем
            Point screenPos = _worldToScreen(_oldWorldX, _oldWorldY);
            Canvas.SetLeft(_element, screenPos.X);
            Canvas.SetTop(_element, screenPos.Y);
            _sensor.X = screenPos.X; 
            _sensor.Y = screenPos.Y;
        }
    }
}
