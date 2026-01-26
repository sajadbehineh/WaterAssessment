using System.ComponentModel.DataAnnotations;

namespace WaterAssessment.Models
{
    public class FormValue
    {
        [Key]
        public int FormValueID { get; set; }

        // ارتباط با هدر (Assessment)
        public int AssessmentID { get; set; }
        public virtual Assessment Assessment { get; set; }

        // ترتیب نمایش در گرید
        public int RowIndex { get; set; }

        // ==========================================
        // ورودی‌های فیزیکی و هندسی
        // ==========================================

        // فاصله از مبدا (ساحل)
        public double Distance { get; set; }

        // عمق کل آب در این نقطه
        public double TotalDepth { get; set; }

        // زمان اندازه‌گیری (این همان فیلدی است که باعث ارور شده بود)
        // حالا در دیتابیس ذخیره می‌شود تا هر سطر زمان خودش را داشته باشد
        public double MeasureTime { get; set; }

        // ==========================================
        // ورودی‌های مولینه (تعداد دور)
        // نال‌پذیر هستند چون شاید کاربر در هر ۳ عمق اندازه‌گیری نکند
        // ==========================================

        public double? Rev02 { get; set; } // دور در عمق 0.2
        public double? Rev06 { get; set; } // دور در عمق 0.6
        public double? Rev08 { get; set; } // دور در عمق 0.8

        // ==========================================
        // مقادیر محاسباتی (ذخیره جهت گزارش‌گیری سریع و تاریخچه)
        // ==========================================

        // سرعت‌های محاسبه شده در نقاط مختلف
        public double? Velocity02 { get; set; }
        public double? Velocity06 { get; set; }
        public double? Velocity08 { get; set; }

        // خروجی‌های نهایی هیدرولیکی این سطر
        public double VerticalMeanVelocity { get; set; } // سرعت متوسط در قائم
        public double SectionWidth { get; set; }         // عرض مقطع (محاسبه از روی همسایه‌ها)
        public double SectionArea { get; set; }          // سطح مقطع (عرض * عمق)
        public double SectionFlow { get; set; }          // دبی عبوری از این مقطع
    }
}
