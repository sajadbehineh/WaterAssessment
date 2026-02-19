using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using System.IO;
using Windows.Foundation;
using WinRT;

namespace WaterAssessment.Services
{
    public class ThemeService : IThemeService
    {
        private const string ThemeSettingKey = "AppTheme";
        private Window _window;
        private FrameworkElement _content;
        private MicaController _micaController;
        private DesktopAcrylicController _acrylicController;
        private SystemBackdropConfiguration _backdropConfiguration;
        private TypedEventHandler<FrameworkElement, object> _actualThemeChangedHandler;

        public Window CurrentWindow => _window;

        public void Initialize(Window window)
        {
            if (window is null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            _window = window;
            _content = _window.Content as FrameworkElement;

            if (_content is null)
            {
                return;
            }

            // آماده‌سازی Configuration
            _backdropConfiguration = new SystemBackdropConfiguration
            {
                IsInputActive = true,
                Theme = (_content?.ActualTheme == ElementTheme.Dark)
                    ? SystemBackdropTheme.Dark
                    : SystemBackdropTheme.Light
            };

            // گوش دادن به تغییر تم

            _actualThemeChangedHandler = (s, e) =>
            {
                if (_backdropConfiguration != null)
                {
                    _backdropConfiguration.Theme = (_content.ActualTheme == ElementTheme.Dark)
                        ? SystemBackdropTheme.Dark
                        : SystemBackdropTheme.Light;
                }
            };

            _content.ActualThemeChanged += _actualThemeChangedHandler;

            _window.Closed -= OnWindowClosed;
            _window.Closed += OnWindowClosed;

            ConfigElementTheme(LoadSavedTheme());

        }

        public void ConfigBackdrop(BackdropType type)
        {
            if (_window == null) return;

            DisposeControllers();

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
                SaveTheme(theme);
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

        public void SetThemeRadioButtonDefaultItem(Panel panel)
        {
            if (_content == null || panel == null)
            {
                return;
            }

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
                        ConfigElementTheme(ElementTheme.Light);
                        break;
                    case "Dark":
                        ConfigElementTheme(ElementTheme.Dark);
                        break;
                    case "Default":
                        ConfigElementTheme(ElementTheme.Default);
                        break;
                }
            }
        }

        private static ElementTheme LoadSavedTheme()
        {
            try
            {
                if (!File.Exists(GetThemeStoragePath()))
                {
                    return ElementTheme.Default;
                }

                var savedValue = File.ReadAllText(GetThemeStoragePath()).Trim();

                return Enum.TryParse(savedValue, out ElementTheme parsedTheme)
                    ? parsedTheme
                    : ElementTheme.Default;
            }
            catch
            {
                return ElementTheme.Default;
            }
        }

        private static void SaveTheme(ElementTheme theme)
        {
            try
            {
                var filePath = GetThemeStoragePath();
                var directoryPath = Path.GetDirectoryName(filePath);

                if (!string.IsNullOrWhiteSpace(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllText(filePath, theme.ToString());
            }
            catch
            {
                // Ignore persistence failures and keep app functional.
            }
        }

        private static string GetThemeStoragePath()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(appDataPath, "WaterAssessment", $"{ThemeSettingKey}.txt");
        }

        private void OnWindowClosed(object sender, WindowEventArgs args)
        {
            if (_content != null && _actualThemeChangedHandler != null)
            {
                _content.ActualThemeChanged -= _actualThemeChangedHandler;
                _actualThemeChangedHandler = null;
            }

            if (_window != null)
            {
                _window.Closed -= OnWindowClosed;
            }

            DisposeControllers();

            _backdropConfiguration = null;
            _content = null;
            _window = null;
        }

        private void DisposeControllers()
        {
            _micaController?.Dispose();
            _micaController = null;

            _acrylicController?.Dispose();
            _acrylicController = null;
        }
    }
}
