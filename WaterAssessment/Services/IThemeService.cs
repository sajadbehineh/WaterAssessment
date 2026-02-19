using Windows.UI;

namespace WaterAssessment.Services
{
    public enum BackdropType
    {
        None,
        Mica,
        Acrylic
    }

    public interface IThemeService
    {
        Window CurrentWindow { get; }

        void Initialize(Window window);
        void ConfigBackdrop(BackdropType type);
        void ConfigElementTheme(ElementTheme theme);
        void ConfigBackdropFallBackBrushForWindow10(Brush brush);
        void SetThemeRadioButtonDefaultItem(Panel panel);
        void OnThemeRadioButtonChecked(object sender);
    }
}
