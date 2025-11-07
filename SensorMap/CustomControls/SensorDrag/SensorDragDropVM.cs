using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SensorMap.CustomControls.SensorDrag
{
    public class SensorDragDropVM:ReactiveObject
    {
        private double _x;
        private double _y;
        private string _removeSensorName;
        public SensorDragDropVM()
        {
            
        }
        
        [Reactive]
        public double X
        {
            get => _x;
            set
            {
                _x = value;
                this.RaiseAndSetIfChanged(ref _x, value);
            }
        }


        [Reactive]
        public double Y
        {
            get
            {
                return _y;
            }
            set
            {
                _y = value;
                this.RaiseAndSetIfChanged(ref _y, value);
            }
        }


        [Reactive]
        public string RemoveSensorName
        {
            get
            {
                return _removeSensorName;
            }
            set
            {
                _removeSensorName = value;
                this.RaiseAndSetIfChanged(ref _removeSensorName, value);
            }
        }

    }
}
