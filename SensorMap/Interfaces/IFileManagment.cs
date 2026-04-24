using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SensorMap.Interfaces
{
    public interface IFileManagment
    {
        /// <summary>
        /// Открыть диалоговое окно
        /// </summary>
        /// <returns>Пути файлов</returns>
        string[] OpenFileDialog();
        /// <summary>
        /// Получить иконку файла
        /// </summary>
        /// <returns>Картинка иконки</returns>
        ImageSource GetIconFile(string path);
    }
}
