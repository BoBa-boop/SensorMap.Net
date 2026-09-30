using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SensorMap.Commands.DataGridCommands
{
    public static class GroupCommands
    {
        public static readonly RoutedUICommand ExpandAll = new("Развернуть все", nameof(ExpandAll), typeof(GroupCommands));
        public static readonly RoutedUICommand CollapseAll = new("Свернуть все", nameof(CollapseAll), typeof(GroupCommands));
    }
}
