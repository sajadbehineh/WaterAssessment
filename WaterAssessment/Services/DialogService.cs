namespace WaterAssessment.Services
{
    public class DialogService : IDialogService
    {
        public XamlRoot? XamlRoot { get; set; }

        public async Task<bool> ShowConfirmationDialogAsync(string title, string content,
            string primaryButtonText = "تایید", string closeButtonText = "انصراف")
        {
            if (XamlRoot == null)
            {
                System.Diagnostics.Debug.WriteLine("XamlRoot is not set on DialogService!");
                return false;
            }

            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText,
                DefaultButton = ContentDialogButton.Close,
                FlowDirection = FlowDirection.RightToLeft,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }
    }
}
