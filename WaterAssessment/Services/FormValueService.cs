namespace WaterAssessment.Services
{
    public class FormValueService : IFormValueService
    {
        public FormValueCalculationResult CalculateVelocities(
            Propeller? propeller,
            double measureTime,
            double totalDepth,
            double? rev02,
            double? rev06,
            double? rev08)
        {
            if (propeller == null)
            {
                return new FormValueCalculationResult(double.NaN, double.NaN, double.NaN, 0);
            }

            if (measureTime <= 0)
            {
                return new FormValueCalculationResult(double.NaN, double.NaN, double.NaN, 0);
            }

            var velocity02 = CalculatePointVelocity(propeller, rev02, measureTime);
            var velocity06 = CalculatePointVelocity(propeller, rev06, measureTime);
            var velocity08 = CalculatePointVelocity(propeller, rev08, measureTime);

            var has02 = !double.IsNaN(velocity02);
            var has06 = !double.IsNaN(velocity06);
            var has08 = !double.IsNaN(velocity08);

            var v02 = has02 ? velocity02 : 0;
            var v06 = has06 ? velocity06 : 0;
            var v08 = has08 ? velocity08 : 0;

            double verticalMeanVelocity;

            if (has02 && has06 && has08)
            {
                verticalMeanVelocity = totalDepth < 3
                    ? (v02 + v06 + v08) / 3.0
                    : (v02 + 2 * v06 + v08) / 4.0;
            }
            else if (has02 && has08)
            {
                verticalMeanVelocity = (v02 + v08) / 2.0;
            }
            else if (has02 && has06)
            {
                verticalMeanVelocity = (v02 + v06) / 2.0;
            }
            else if (has06 && has08)
            {
                verticalMeanVelocity = (v06 + v08) / 2.0;
            }
            else if (has06)
            {
                verticalMeanVelocity = v06;
            }
            else if (has02)
            {
                verticalMeanVelocity = v02;
            }
            else if (has08)
            {
                verticalMeanVelocity = v08;
            }
            else
            {
                verticalMeanVelocity = 0;
            }

            return new FormValueCalculationResult(velocity02, velocity06, velocity08, verticalMeanVelocity);
        }

        private static double CalculatePointVelocity(Propeller propeller, double? rev, double measureTime)
        {
            if (!rev.HasValue)
            {
                return double.NaN;
            }

            var n = rev.Value > 0 ? rev.Value / measureTime : 0;
            return propeller.CalculateVelocity(n);
        }
    }
}

