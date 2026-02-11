namespace WaterAssessment.Services
{
    public interface IFormValueViewModelFactory
    {
        FormValueViewModel Create(FormValue model, Propeller propeller);
    }
}
