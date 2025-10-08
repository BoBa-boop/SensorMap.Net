using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace SensorMap.Converters
{
    public class EnumBinding:MarkupExtension
    {
        public Type EnumType { get; private set; }
        public EnumBinding(Type type) 
        {
            if (type is null || !type.IsEnum) return;
            EnumType = type;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(EnumType);
        }
    }
}
