using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WaterAssessment.Models.ViewModel
{
    public partial class FormValueViewModel : ObservableObject
    {
        public FormValue Model { get; }
        private readonly Propeller _propeller;

        public FormValueViewModel(FormValue model, Propeller propeller)
        {
            Model = model;
            _propeller = propeller;

            // پر کردن مقادیر اولیه ویومدل از روی مدل
            _measureTime = model.MeasureTime;
            _distance = model.Distance;
            _totalDepth = model.TotalDepth;

            // تبدیل int? مدل به double برای استفاده در NumberBox
            // اگر نال بود، NaN میگذاریم که نامبرباکس خالی نشان دهد
            _rev02 = model.Rev02.HasValue ? model.Rev02.Value : double.NaN;
            _rev06 = model.Rev06.HasValue ? model.Rev06.Value : double.NaN;
            _rev08 = model.Rev08.HasValue ? model.Rev08.Value : double.NaN;

            // اگر مقادیر از قبل در مدل بود، محاسبات اولیه انجام شود
            if (model.MeasureTime > 0)
            {
                CalculateVelocities();
            }
        }

        public int RowIndex => Model.RowIndex;
        public bool IsFirstRow => RowIndex == 1;
        public double SectionGridHeight => IsFirstRow ? 37 : 74;
        public VerticalAlignment SectionGridAlignment => VerticalAlignment.Top;
        public Visibility SectionGridVisibility => IsFirstRow ? Visibility.Collapsed : Visibility.Visible;
        public Thickness SectionGridMargin => IsFirstRow ? new Thickness(0) : new Thickness(0, 0, 0, -37);
        public double SectionTranslationY => IsFirstRow ? 0 : -37;
        public bool ShowCalculationGrid => !IsFirstRow;

        // ==========================================================
        // 1. ورودی‌های هندسی (فاصله و عمق کل)
        // ==========================================================

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SectionWidth))]
        private double _distance;

        partial void OnDistanceChanged(double value)
        {
            double rounded = Math.Round(value, 2);

            if (Math.Abs(value - rounded) > 0.00001)
            {
                Distance = rounded; // اصلاح مقدار در UI
                return; // خروج موقت (چون تغییر Distance دوباره این متد را صدا می‌زند)
            }

            Model.Distance = value;
        }


        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SectionArea))] // تغییر عمق -> تغییر سطح
        [NotifyPropertyChangedFor(nameof(SectionFlow))] // تغییر عمق -> تغییر دبی
        private double _totalDepth;

        partial void OnTotalDepthChanged(double value)
        {
            Model.TotalDepth = value;
            CalculateResults();
        }

        // ==========================================================
        // 2. ورودی‌های اندازه‌گیری (زمان و دورها)
        // ==========================================================

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(N02Display))]
        [NotifyPropertyChangedFor(nameof(N06Display))]
        [NotifyPropertyChangedFor(nameof(N08Display))]
        private double _measureTime;

        partial void OnMeasureTimeChanged(double value)
        {
            Model.MeasureTime = value;
            CalculateVelocities(); // تغییر زمان -> تغییر سرعت‌ها
        }

        // تعداد دور در نقطه 0.2
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(N02Display))]
        private double _rev02;
        partial void OnRev02Changed(double value)
        {
            // تبدیل double به int? برای مدل
            Model.Rev02 = double.IsNaN(value) ? null : (int)value;
            CalculateVelocities();
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(N06Display))]
        private double _rev06;
        partial void OnRev06Changed(double value)
        {
            Model.Rev06 = double.IsNaN(value) ? null : (int)value;
            CalculateVelocities();
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(N08Display))]
        private double _rev08;
        partial void OnRev08Changed(double value)
        {
            Model.Rev08 = double.IsNaN(value) ? null : (int)value;
            CalculateVelocities();
        }


        // ==========================================================
        // 3. خروجی‌های محاسباتی (فقط جهت نمایش)
        // ==========================================================

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(VerticalMeanVelocityDisplay))]
        private double _verticalMeanVelocity; // سرعت متوسط قائم
        public string VerticalMeanVelocityDisplay => VerticalMeanVelocity.ToString("N3"); // نمایش با 3 رقم اعشار

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SectionWidthDisplay))] // اطلاع به UI برای آپدیت متن
        private double _sectionWidth;

        public string SectionWidthDisplay
        {
            get
            {
                // اگر سطر اول است، خالی نشان بده
                if (IsFirstRow) return string.Empty;

                // در غیر این صورت، عدد را با 2 رقم اعشار نشان بده
                return SectionWidth.ToString("N2");
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SectionAreaDisplay))]
        [NotifyPropertyChangedFor(nameof(SectionFlowDisplay))]
        private double _sectionArea;

        public string SectionAreaDisplay
        {
            get
            {
                // اگر سطر اول است، چیزی نشان نده (مثل اکسل)
                if (IsFirstRow) return string.Empty;

                return SectionArea.ToString("N2");
            }
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SectionArea))]
        [NotifyPropertyChangedFor(nameof(PanelAvgVelocity))]
        [NotifyPropertyChangedFor(nameof(SectionFlowDisplay))]
        private double _sectionFlow;

        public string SectionFlowDisplay
        {
            get
            {
                // اگر سطر اول است، چیزی نشان نده
                if (IsFirstRow) return string.Empty;

                return SectionFlow.ToString("N3");
            }
        }

        // --- عمق میانگین پنل ---
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PanelAvgDepthDisplay))]
        private double _panelAvgDepth;
        public string PanelAvgDepthDisplay
        {
            get
            {
                if (IsFirstRow) return string.Empty;
                return PanelAvgDepth.ToString("N2");
            }
        }

        // 2. میانگین سرعت (Average Velocity)
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(PanelAvgVelocityDisplay))]
        private double _panelAvgVelocity;

        // پراپرتی نمایشی (بایند به XAML)
        public string PanelAvgVelocityDisplay
        {
            get
            {
                // اگر سطر اول است، هیچی نشان نده (چون میانگین با قبلی معنا ندارد)
                if (IsFirstRow) return string.Empty;

                // در غیر این صورت مقدار را با 3 رقم اعشار نشان بده
                return PanelAvgVelocity.ToString("N3");
            }
        }

        // ==========================================
        // پراپرتی‌های نمایشی برای دور در ثانیه (n = N / t)
        // ==========================================

        public string N02Display
        {
            get
            {
                // اگر زمان صفر است یا دوری وارد نشده، خط تیره یا خالی نشان بده
                if (MeasureTime <= 0 || double.IsNaN(Rev02)) return "-";

                // محاسبه n و نمایش با 3 رقم اعشار
                return (Rev02 / MeasureTime).ToString("N3");
            }
        }

        public string N06Display => (MeasureTime <= 0 || double.IsNaN(Rev06))
            ? "-"
            : (Rev06 / MeasureTime).ToString("N3");

        public string N08Display => (MeasureTime <= 0 || double.IsNaN(Rev08))
            ? "-"
            : (Rev08 / MeasureTime).ToString("N3");

        // ==========================================
        // سرعت‌های نقطه‌ای (محاسبه شده توسط پروانه)
        // ==========================================

        // نقطه 0.2
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Velocity02Display))]
        private double _velocity02 = double.NaN; // پیش‌فرض NaN یعنی محاسبه نشده

        public string Velocity02Display => double.IsNaN(Velocity02) ? "-" : Velocity02.ToString("N3");

        // نقطه 0.6
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Velocity06Display))]
        private double _velocity06 = double.NaN;

        public string Velocity06Display => double.IsNaN(Velocity06) ? "-" : Velocity06.ToString("N3");

        // نقطه 0.8
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Velocity08Display))]
        private double _velocity08 = double.NaN;

        public string Velocity08Display => double.IsNaN(Velocity08) ? "-" : Velocity08.ToString("N3");


        // ==========================================================
        // 4. منطق محاسبات (Business Logic)
        // ==========================================================

        private void CalculateVelocities()
        {
            // 1. اگر زمان صفر است، همه سرعت‌ها صفر یا نامعتبر می‌شوند
            if (MeasureTime <= 0)
            {
                Velocity02 = double.NaN;
                Velocity06 = double.NaN;
                Velocity08 = double.NaN;
                VerticalMeanVelocity = 0;
                return;
            }

            // 2. محاسبه سرعت نقطه 0.2
            if (!double.IsNaN(Rev02)) // فقط اگر کاربر عددی وارد کرده باشد
            {
                double n = (Rev02 > 0) ? Rev02 / MeasureTime : 0;
                Velocity02 = _propeller.CalculateVelocity(n);
            }
            else
            {
                Velocity02 = double.NaN; // خالی بماند
            }

            // 3. محاسبه سرعت نقطه 0.6
            if (!double.IsNaN(Rev06))
            {
                double n = (Rev06 > 0) ? Rev06 / MeasureTime : 0;
                Velocity06 = _propeller.CalculateVelocity(n);
            }
            else
            {
                Velocity06 = double.NaN;
            }

            // 4. محاسبه سرعت نقطه 0.8
            if (!double.IsNaN(Rev08))
            {
                double n = (Rev08 > 0) ? Rev08 / MeasureTime : 0;
                Velocity08 = _propeller.CalculateVelocity(n);
            }
            else
            {
                Velocity08 = double.NaN;
            }

            // 5. میانگین‌گیری (از مقادیر پراپرتی‌ها استفاده می‌کنیم)
            // نکته: اگر Velocity02 مقدار NaN باشد، یعنی آن نقطه اندازه‌گیری نشده است
            bool has02 = !double.IsNaN(Velocity02);
            bool has06 = !double.IsNaN(Velocity06);
            bool has08 = !double.IsNaN(Velocity08);

            double vMean = 0;

            // توجه: وقتی مقدار NaN است، در جمع ریاضی نتیجه را NaN می‌کند
            // بنابراین باید از مقادیر عددی یا 0 استفاده کنیم
            double val02 = has02 ? Velocity02 : 0;
            double val06 = has06 ? Velocity06 : 0;
            double val08 = has08 ? Velocity08 : 0;

            if (has02 && has08)
            {
                if (has06)
                    vMean = (val02 + 2 * val06 + val08) / 4; // سه نقطه‌ای
                else
                    vMean = (val02 + val08) / 2; // دو نقطه‌ای
            }
            else if (has06)
            {
                vMean = val06; // تک نقطه‌ای
            }
            else if (has02)
            {
                vMean = val02; // فقط سطح
            }

            // 6. ذخیره نهایی
            VerticalMeanVelocity = vMean;
            Model.VerticalMeanVelocity = vMean;
        }

        private void CalculateResults()
        {
            // سطح = عرض * عمق کل
            //SectionArea = SectionWidth * TotalDepth;
            //Model.SectionArea = SectionArea;

            //// دبی = سطح * سرعت متوسط
            //SectionFlow = SectionArea * VerticalMeanVelocity;
            //Model.SectionFlow = SectionFlow;
        }

        /// <summary>
        /// این متد توسط ویومدل پدر (Assessment) صدا زده می‌شود تا عرض را ست کند
        /// </summary>
        public void UpdateSectionWidth(double prevDist, double nextDist)
        {
            SectionWidth = (nextDist - prevDist) / 2;
            Model.SectionWidth = SectionWidth;

            // چون عرض تغییر کرد، محاسبات نهایی تکرار شود
            CalculateResults();
        }
    }
}
