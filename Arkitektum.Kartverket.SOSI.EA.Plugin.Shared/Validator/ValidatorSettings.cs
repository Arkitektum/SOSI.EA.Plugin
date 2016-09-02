namespace arkitektum.gistools.generators.Validator
{
    public class ValidatorSettings
    {
        public bool ValidateForGml { get; }
        public bool ValidateForSosi { get; }

        public ValidatorSettings(bool validateForGml, bool validateForSosi)
        {
            ValidateForGml = validateForGml;
            ValidateForSosi = validateForSosi;
        }
    }
}
