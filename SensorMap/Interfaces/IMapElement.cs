using SensorMap.Model;
using System.Windows;

namespace SensorMap.Interfaces
{
    /// <summary>
    /// Интерфейс для UI-элементов на карте (CustomSensor, CustomDevice)
    /// </summary>
    public interface IMapElement
    {
        MapObject MapData { get; }
        void SetCustomBounds(Rect bounds);
        Rect GetCustomBounds();
    }
}
