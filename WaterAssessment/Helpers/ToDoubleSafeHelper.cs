namespace WaterAssessment.Helpers
{
    public class ToDoubleSafeHelper
    {
        public static double ToDoubleSafe(string text)
        {
            // اگر تبدیل موفق بود عدد را برگردان، در غیر این صورت صفر
            return double.TryParse(text, out double result) ? result : 0.0;
        }
    }
}
