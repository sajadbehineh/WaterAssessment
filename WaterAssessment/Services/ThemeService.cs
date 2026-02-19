using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using WinRT;

namespace WaterAssessment.Services
{
    public class ThemeService : IThemeService
    {
        private Window _window;
        private FrameworkElement _content;
        private MicaController _micaController;
        private DesktopAcrylicController _acrylicController;
        private SystemBackdropConfiguration _backdropConfiguration;

        public Window CurrentWindow => _window;

        public void Initialize(Window window)
        {
            _window = window;
            _content = _window.Content as FrameworkElement;

            // آماده‌سازی Configuration
            _backdropConfiguration = new SystemBackdropConfiguration
            {
                IsInputActive = true,
                Theme = (_content?.ActualTheme == ElementTheme.Dark)
                    ? SystemBackdropTheme.Dark
                    : SystemBackdropTheme.Light
            };

            // گوش دادن به تغییر تم
            if (_content != null)
            {
                _content.ActualThemeChanged += (s, e) =>
                {
                    _backdropConfiguration.Theme = (_content.ActualTheme == ElementTheme.Dark)
                        ? SystemBackdropTheme.Dark
                        : SystemBackdropTheme.Light;
                };
            }
        }

        public void ConfigBackdrop(BackdropType type)
        {
            if (_window == null) return;

            if (type == BackdropType.Mica && MicaController.IsSupported())
            {
                _micaController = new MicaController();
                _micaController.AddSystemBackdropTarget(_window.As<ICompositionSupportsSystemBackdrop>());
                _micaController.SetSystemBackdropConfiguration(_backdropConfiguration);
            }
            else if (type == BackdropType.Acrylic && DesktopAcrylicController.IsSupported())
            {
                _acrylicController = new DesktopAcrylicController();
                _acrylicController.AddSystemBackdropTarget(_window.As<ICompositionSupportsSystemBackdrop>());
                _acrylicController.SetSystemBackdropConfiguration(_backdropConfiguration);
            }
            else
            {
                // fallback برای Windows 10: Background ساده
                ConfigBackdropFallBackBrushForWindow10(brush: new SolidColorBrush(Colors.White));
            }
        }

        public void ConfigElementTheme(ElementTheme theme)
        {
            if (_content != null)
            {
                _content.RequestedTheme = theme;
            }
        }

        public void ConfigBackdropFallBackBrushForWindow10(Brush brush)
        {
            if (_content == null || brush == null) return;

            if (_content is Control ctrl) ctrl.Background = brush;
            else if (_content is Panel panel) panel.Background = brush;
            else
            {
                var grid = new Grid { Background = brush };
                var old = _window.Content;
                _window.Content = grid;
                if (old is UIElement el) grid.Children.Add(el);
                _content = grid;
            }
        }

        //private void ApplyBackgroundFallback(Brush brush)
        //{
        //    if (_content == null || brush == null) return;

        //    if (_content is Control ctrl) ctrl.Background = brush;
        //    else if (_content is Panel panel) panel.Background = brush;
        //    else
        //    {
        //        var grid = new Grid { Background = brush };
        //        var old = _window.Content;
        //        _window.Content = grid;
        //        if (old is UIElement el) grid.Children.Add(el);
        //        _content = grid;
        //    }
        //}

        public void SetThemeRadioButtonDefaultItem(Panel panel)
        {
            foreach (var child in panel.Children)
            {
                if (child is RadioButton rb)
                {
                    if (_content.RequestedTheme == ElementTheme.Light && rb.Tag?.ToString() == "Light")
                        rb.IsChecked = true;
                    else if (_content.RequestedTheme == ElementTheme.Dark && rb.Tag?.ToString() == "Dark")
                        rb.IsChecked = true;
                    else if (_content.RequestedTheme == ElementTheme.Default && rb.Tag?.ToString() == "Default")
                        rb.IsChecked = true;
                }
            }
        }

        public void OnThemeRadioButtonChecked(object sender)
        {
            if (sender is RadioButton rb && _content != null)
            {
                switch (rb.Tag?.ToString())
                {
                    case "Light":
                        _content.RequestedTheme = ElementTheme.Light;
                        break;
                    case "Dark":
                        _content.RequestedTheme = ElementTheme.Dark;
                        break;
                    case "Default":
                        _content.RequestedTheme = ElementTheme.Default;
                        break;
                }
            }
        }
    }
}
