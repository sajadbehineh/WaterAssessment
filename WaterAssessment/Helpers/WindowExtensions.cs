using Windows.Graphics;
using Microsoft.UI.Windowing;

namespace WaterAssessment.Helpers
{
    public static class WindowExtensions
    {
        public static void SetWindowSize(this Window window, int width, int height)
        {
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var windowId = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(hwnd));
            if (windowId != null)
            {
                windowId.Resize(new SizeInt32(width, height));
            }
        }
    }
}
