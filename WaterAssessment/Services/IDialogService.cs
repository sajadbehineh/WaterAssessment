namespace WaterAssessment.Services
{
    public interface IDialogService
    {
        Task<bool> ShowConfirmationDialogAsync(string title, string content, string primaryButtonText = "تایید",
            string closeButtonText = "انصراف");
    }
}
