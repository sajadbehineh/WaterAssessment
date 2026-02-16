using CommunityToolkit.Mvvm.ComponentModel;

namespace WaterAssessment.ViewModel
{
    public partial class HydrometrySectionViewModel : ObservableObject
    {
        public int SectionNumber { get; }
        public ObservableCollection<FormValueViewModel> Rows { get; } = new();

        [ObservableProperty]
        private double _sectionFlow;

        public string SectionFlowDisplay => SectionFlow.ToString("N3");
        public string SectionFlowLabel => $"دبی مقطع {SectionNumber} (m3/s):";

        public HydrometrySectionViewModel(int sectionNumber)
        {
            SectionNumber = sectionNumber;
        }

        partial void OnSectionFlowChanged(double value)
        {
            OnPropertyChanged(nameof(SectionFlowDisplay));
        }
    }
}
