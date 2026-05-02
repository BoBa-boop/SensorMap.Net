using SensorMap.EF;
using SensorMap.Model;
using SensorMap.Services;
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
        string[] OpenFileDialog(bool multiselect = false);
        /// <summary>
        /// Получить иконку файла
        /// </summary>
        /// <returns>Картинка иконки</returns>
        ImageSource GetIconFile(string path);

        bool AddHelpfulFile(ITempImage imgManag,object Entity, bool multiselect = false);
        void OpenFileInExplorer(string path);
    }
}
