using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.ComponentModel.DataAnnotations;

namespace SensorMap.Model
{
    /// <summary>
    /// Описывает датчик (тип, картинка, название)
    /// </summary>
    public class Sensor:ReactiveObject
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public SensorType Type { get; set; }
        public string Image { get; set; } = string.Empty;

        public enum SensorType
        {
            Оптический = 1,
            Индуктивный,
            Геркон,
            Концевик,
            Давления,
            Линейка,
            Лазерный,
            Энкодер
        }
    }
    
}
