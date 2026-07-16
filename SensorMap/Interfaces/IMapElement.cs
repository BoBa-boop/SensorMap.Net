using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SensorMap.Interfaces
{
    public interface IMapElement
    {
        FrameworkElement Element { get; }
        MapObject MapData { get; }
        void SetCustomBounds(Rect bounds);
        Rect GetCustomBounds();
    }
}
