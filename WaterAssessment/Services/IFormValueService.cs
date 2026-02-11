namespace WaterAssessment.Services
{
    public interface IFormValueService
    {
        FormValueCalculationResult CalculateVelocities(
            Propeller? propeller, double measureTime, double totalDepth, double? rev02, double? rev06, double? rev08);
    }

    public sealed record FormValueCalculationResult(
        double Velocity02,
        double Velocity06,
        double Velocity08,
        double VerticalMeanVelocity);
}
