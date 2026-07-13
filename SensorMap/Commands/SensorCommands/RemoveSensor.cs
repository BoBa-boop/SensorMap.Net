using SensorMap.Interfaces;
using SensorMap.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SensorMap.Commands.SensorCommands
{
    public class RemoveSensor : IUndoRedoCommand
    {
        private readonly List<IMapElement> _elements;
        private readonly Canvas _canvas;
        private readonly ObservableCollection<MapObject> _collection;
        public RemoveSensor(List<IMapElement> elements, Canvas canvas,
        ObservableCollection<MapObject> collection)
        {
            _elements = elements;//.Select(x => (IMapElement)x.MapData.Clone()).ToList();
            _canvas = canvas;
            _collection = collection;
        }
        public void Do()
        {
            foreach (var element in _elements)
            {
                var item = element.MapData.FindInCanvas(_canvas);
                if (item is null) return;
                element.MapData.ToDelete=true;
                _canvas.Children.Remove(item);
                //if (_collection.Contains(item.SensorData))
                //{
                //    _collection.Remove(item.SensorData);
                //}
            }
        }

        public void Undo()
        {
            
            foreach (var element in _elements.Where(x=>x.MapData.ToDelete==true))
            {
                element.MapData.ToDelete = false;
                _canvas.Children.Add(element.Element);
                Canvas.SetLeft(element.Element, element.MapData.X);
                Canvas.SetTop(element.Element, element.MapData.Y);
                //_collection.Add(item.SensorData);
            }
        }
    }
}
