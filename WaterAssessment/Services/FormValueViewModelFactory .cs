namespace WaterAssessment.Services
{
    public class FormValueViewModelFactory: IFormValueViewModelFactory
    {
        private readonly IFormValueService _formValueService;

        public FormValueViewModelFactory(IFormValueService formValueService)
        {
            _formValueService = formValueService;
        }

        public FormValueViewModel Create(FormValue model, Propeller propeller)
        {
            return new FormValueViewModel(model, propeller, _formValueService);
        }
    }
}
