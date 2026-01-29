namespace WaterAssessment.Models;

public class Propeller
{
    public int PropellerID { get; set; }
    public string PropellerName { get; set; }

    // نوع کالیبراسیون
    public CalibrationMode Mode { get; set; }

    // معادله اول (همیشه استفاده می‌شود)
    public double A1 { get; set; }
    public double B1 { get; set; }

    // نقطه شکست اول (برای جدا کردن معادله ۱ و ۲)
    public double? TransitionPoint1 { get; set; }

    // معادله دوم (برای حالت‌های ۲ و ۳ معادله‌ای)
    public double? A2 { get; set; }
    public double? B2 { get; set; }

    // نقطه شکست دوم (برای جدا کردن معادله ۲ و ۳)
    public double? TransitionPoint2 { get; set; }

    // معادله سوم (فقط برای حالت ۳ معادله‌ای)
    public double? A3 { get; set; }
    public double? B3 { get; set; }

    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserID { get; set; }
    public virtual User CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? UpdatedByUserID { get; set; } // کلید خارجی
    public virtual User UpdatedBy { get; set; }

    public virtual List<Assessment> Assessments { get; set; } = new();

    // متد محاسبه سرعت بر اساس تعداد معادلات انتخاب شده
    public double CalculateVelocity(double n)
    {
        if (n <= 0) return 0;
        switch (Mode)
        {
            case CalibrationMode.OneEquation:
                // همیشه از معادله اول استفاده کن
                return (A1 * n) + B1;

            case CalibrationMode.TwoEquations:
                // اگر کمتر از نقطه شکست ۱ بود -> معادله ۱، وگرنه -> معادله ۲
                if (TransitionPoint1.HasValue && n <= TransitionPoint1.Value)
                    return (A1 * n) + B1;
                else
                    return ((A2 ?? 0) * n) + (B2 ?? 0);

            case CalibrationMode.ThreeEquations:
                // بررسی بازه‌های مختلف
                if (TransitionPoint1.HasValue && n <= TransitionPoint1.Value)
                {
                    return (A1 * n) + B1;
                }
                else if (TransitionPoint2.HasValue && n <= TransitionPoint2.Value)
                {
                    return (A2 ?? 0 * n) + (B2 ?? 0);
                }
                else
                {
                    return ((A3 ?? 0) * n) + (B3 ?? 0);
                }

            default:
                return 0;
        }
    }
}

/// این Enum مشخص می‌کند که پروانه چند نقطه شکست دارد
public enum CalibrationMode
{
    OneEquation = 0,   // فقط یک معادله (بدون نقطه شکست)
    TwoEquations = 1,  // دو معادله (یک نقطه شکست)
    ThreeEquations = 2 // سه معادله (دو نقطه شکست)
}