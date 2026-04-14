using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Size = System.Windows.Size;

namespace SensorMap.Model
{
    public static class MapProperties
    {
        public static readonly DependencyProperty ViewMatrixProperty =
        DependencyProperty.RegisterAttached(
            "ViewMatrix",
            typeof(Matrix),
            typeof(MapProperties),
            new FrameworkPropertyMetadata(
                Matrix.Identity,
                FrameworkPropertyMetadataOptions.Inherits));
        
        public static Matrix GetViewMatrix(DependencyObject obj)
        {
            return (Matrix)obj.GetValue(ViewMatrixProperty);
        }

        public static void SetViewMatrix(DependencyObject obj, Matrix value)
        {
            obj.SetValue(ViewMatrixProperty, value);
        }
    }
}
