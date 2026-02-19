using Microsoft.UI.Windowing;
using WinRT.Interop;

namespace WaterAssessment;

public sealed partial class AssessmentInputsWindow : Window
{
    private AppWindow _appWindow;

    public AssessmentInputsWindow()
    {
        this.InitializeComponent();
        //rootBorder.XamlRoot = (App.Current.m_window).Content.XamlRoot;
        ConfigWindow();
    }

    private void ConfigWindow()
    {
       
        _appWindow = GetAppWindowForCurrentWindow();
    }

    private AppWindow GetAppWindowForCurrentWindow()
    {
        IntPtr hWnd = WindowNative.GetWindowHandle(this);
        WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
        return AppWindow.GetFromWindowId(wndId);
    }
}