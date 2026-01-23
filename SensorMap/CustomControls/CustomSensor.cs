using CommunityToolkit.Mvvm.Input;
using SensorMap.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SensorMap.CustomControls
{
    
    public class CustomSensor : Control
    {
        public double CustomHeight
        {
            get { return (double)GetValue(CHeightProperty); }
            set { SetValue(CHeightProperty, value); }
        }
        public static readonly DependencyProperty CHeightProperty =
            DependencyProperty.Register("CustomHeight", typeof(double), typeof(CustomSensor), new PropertyMetadata(30.0));


        public double CustomWidth
        {
            get { return (double)GetValue(CustomWidthProperty); }
            set { SetValue(CustomWidthProperty, value); }
        }
        public static readonly DependencyProperty CustomWidthProperty =
            DependencyProperty.Register("CustomWidth", typeof(double), typeof(CustomSensor), new PropertyMetadata(30.0));



        public SensorAssignments SensorData
        {
            get { return (SensorAssignments)GetValue(SensorProperty); }
            set { SetValue(SensorProperty, value); }
        }

        public static readonly DependencyProperty SensorProperty =
            DependencyProperty.Register("Sensor", typeof(SensorAssignments), typeof(CustomSensor));


        public char Letter
        {
            get { return (char)GetValue(LetterProperty); }
            set { SetValue(LetterProperty, value); }
        }

        
        public static readonly DependencyProperty LetterProperty =
            DependencyProperty.Register("Letter", typeof(char), typeof(CustomSensor), new PropertyMetadata('-'));


        public SolidColorBrush CustomBackground
        {
            get { return (SolidColorBrush)GetValue(CustomBackgroundProperty); }
            set { SetValue(CustomBackgroundProperty, value); }
        }

        public static readonly DependencyProperty CustomBackgroundProperty =
            DependencyProperty.Register(
                "CustomBackground", 
                typeof(SolidColorBrush), 
                typeof(CustomSensor), 
                new PropertyMetadata(new SolidColorBrush(Colors.Red)));



        public SolidColorBrush BorderBrush
        {
            get { return (SolidColorBrush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(SolidColorBrush), typeof(CustomSensor), new PropertyMetadata(Brushes.Black));


        public bool ShowAddresses
        {
            get { return (bool)GetValue(ShowAddressesProperty); }
            set { SetValue(ShowAddressesProperty, value);}
        }
        public static readonly DependencyProperty ShowAddressesProperty =
            DependencyProperty.Register("ShowAddresses", typeof(bool), typeof(CustomSensor), new PropertyMetadata(false));
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); SelectedChanged(); }
        }
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(CustomSensor), new PropertyMetadata(false));

        

        static CustomSensor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomSensor), new FrameworkPropertyMetadata(typeof(CustomSensor)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Letter = SensorData.Sensor.SensorType.Name.ToUpper().First();
            
        }
        private void SelectedChanged()
        {
            BorderBrush = IsSelected ? Brushes.DarkGreen : Brushes.Black;
        }
    }
}
