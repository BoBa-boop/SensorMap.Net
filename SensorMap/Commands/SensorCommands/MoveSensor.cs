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
        private readonly SensorAssignments _sensor;
        private readonly Border _element;
        private readonly double _oldX;
        private readonly double _oldY;
        private readonly double _newX;
        private readonly double _newY;

        public MoveSensor(SensorAssignments sensor,Border element, double newX, double newY)
        {
            _element = element;
            _sensor = sensor;
            _oldX = sensor.X;
            _oldY = sensor.Y;
            _newX = newX;
            _newY = newY;
        }

        public void Do()
        {
            _sensor.X = _newX;
            _sensor.Y = _newY; 
            Canvas.SetLeft(_element, _sensor.X);
            Canvas.SetTop(_element, _sensor.Y);
            //MessageBox.Show($"New: {_sensor.X};{_sensor.Y}\rOld:{_oldX};{_oldY}");
        }

        public void Undo()
        {
            _sensor.X = _oldX;
            _sensor.Y = _oldY;
            Canvas.SetLeft( _element, _sensor.X );
            Canvas.SetTop(_element, _sensor.Y );
        }
    }
}
