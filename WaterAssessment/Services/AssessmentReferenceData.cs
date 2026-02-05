namespace WaterAssessment.Services
{
    public class AssessmentReferenceData
    {
        public IEnumerable<Location> Locations { get; set; } = Enumerable.Empty<Location>();
        public IEnumerable<Propeller> Propellers { get; set; } = Enumerable.Empty<Propeller>();
        public IEnumerable<CurrentMeter> CurrentMeters { get; set; } = Enumerable.Empty<CurrentMeter>();
        public IEnumerable<Employee> Employees { get; set; } = Enumerable.Empty<Employee>();
    }
}
