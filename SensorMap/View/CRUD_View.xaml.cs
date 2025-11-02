using System.Windows;
using System.Windows.Controls;

namespace SensorMap.View
{
    /// <summary>
    /// Логика взаимодействия для CRUD_View.xaml
    /// </summary>
    public partial class CRUD_View : UserControl
    {
        public CRUD_View()
        {
            InitializeComponent();
        }

        private void DataGrid_Unloaded(object sender, RoutedEventArgs e)
        {
            var grid = (DataGrid)sender;
            grid.CancelEdit(DataGridEditingUnit.Row);
        }
    }
}
