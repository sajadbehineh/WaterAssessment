using Windows.Globalization.NumberFormatting;

namespace WaterAssessment.Helpers
{
    public class DoubleFormatterF1 : INumberFormatter2, INumberParser
    {
        public virtual string Format { get; set; } = "{0:F1}"; // by default we use this but you can change it in the XAML declaration
        public virtual string FormatDouble(double value) => string.Format(Format, value);
        public virtual double? ParseDouble(string text) => double.TryParse(text, out var dbl) ? dbl : null;

        // we only support doubles
        public string FormatInt(long value) => throw new NotSupportedException();
        public string FormatUInt(ulong value) => throw new NotSupportedException();
        public long? ParseInt(string text) => throw new NotSupportedException();
        public ulong? ParseUInt(string text) => throw new NotSupportedException();
    }
}
