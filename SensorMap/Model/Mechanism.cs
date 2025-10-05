using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

namespace SensorMap.Model
{
    /// <summary>
    /// Описывает оборудование
    /// </summary>
    public class Mechanism : ReactiveObject
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;//путь до папки с данными
        public string Image { get; set; } = string.Empty;
        public int SectorID { get; set; }
        public Sector? Sector { get; set; }//ссылка для EF
        public PLC? PLC { get; set; }//ссылка для EF

    }
}
