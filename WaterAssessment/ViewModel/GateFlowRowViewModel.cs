using CommunityToolkit.Mvvm.ComponentModel;

namespace WaterAssessment.ViewModel;

public partial class GateFlowRowViewModel : ObservableObject
{
    private const double G = 9.81;

    public HydraulicGate Gate { get; }

    public int HydraulicGateID => Gate.Id;
    public int GateNumber => Gate.GateNumber;
    public double DischargeCoefficient => Gate.DischargeCoefficient;
    public double Width => Gate.Width;

    [ObservableProperty]
    private double _openingHeight;

    [ObservableProperty]
    private double _upstreamHead;

    [ObservableProperty]
    private double _calculatedFlow;

    public event Action? RowChanged;

    public GateFlowRowViewModel(HydraulicGate gate, double openingHeight = 0, double waterHead = 0)
    {
        Gate = gate;
        _openingHeight = openingHeight;
        _upstreamHead = waterHead;
        Recalculate();
    }

    partial void OnOpeningHeightChanged(double value) => Recalculate();
    partial void OnUpstreamHeadChanged(double value) => Recalculate();

    private void Recalculate()
    {
        var h = Math.Max(0, OpeningHeight);
        var H = Math.Max(0, UpstreamHead);
        CalculatedFlow = DischargeCoefficient * Width * h * Math.Sqrt(2 * G * H);
        RowChanged?.Invoke();
    }
}

