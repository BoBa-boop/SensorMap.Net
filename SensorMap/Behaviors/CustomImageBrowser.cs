using HandyControl.Interactivity;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Image = System.Windows.Controls.Image;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace SensorMap.Behaviors
{
    [TemplatePart(Name = "PART_PanelTop", Type = typeof(Border))]
    public class CustomImageBrowser:Window
    {
        private const string ElementPanelTop = "PART_PanelTop";
        private Border? _panelTop;
        static CustomImageBrowser()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomImageBrowser),
                new FrameworkPropertyMetadata(typeof(CustomImageBrowser)));
        }
        public CustomImageBrowser(ImageSource imageSource) : base()
        {
            base.CommandBindings.Add(new CommandBinding(ControlCommands.Close, ButtonClose_OnClick));
            base.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            base.WindowStyle = WindowStyle.None;
            base.AllowsTransparency = true;
            InitializeWithImageSource(imageSource);
        }

        private void ButtonClose_OnClick(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void InitializeWithImageSource(ImageSource imageSource)
        {
            // Создаем новую структуру
            var newImage = new Image
            {
                Source = imageSource,
                Stretch = Stretch.Uniform,
                Width = 1080,
                Height = 720

            };
            this.Content = newImage;
        }
        public override void OnApplyTemplate()
        {
            if (_panelTop != null)
            {
                _panelTop.MouseLeftButtonDown -= PanelTopOnMouseLeftButtonDown;
            }

            base.OnApplyTemplate();
            _panelTop = GetTemplateChild("PART_PanelTop") as Border;
            if (_panelTop != null)
            {
                _panelTop.MouseLeftButtonDown += PanelTopOnMouseLeftButtonDown;
            }

        }

        private void PanelTopOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            base.OnKeyDown(e);
        }
    }
}
