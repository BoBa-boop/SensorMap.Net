using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SensorMap.Model.TreeNode
{
    public class SensorsTreeNode:ReactiveObject
    {
        [Reactive] public string Name { get; set; } = string.Empty;
        [Reactive] public byte[]? Image { get; set; } 
        [Reactive] public ObservableCollection<Sensor> Children { get; set; } = new();
    }
}
