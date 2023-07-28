namespace WaterAssessment.Helpers;

public static class InitInfoBar
{
    public static void ImplementInfoBar(InfoBar infoBar, InfoBarSeverity severity, bool isOpen, string message)
    {
        infoBar.Severity = severity;
        infoBar.Message = message;
        infoBar.IsOpen = isOpen;
    }
}