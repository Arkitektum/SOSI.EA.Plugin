using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EA;
using static arkitektum.gistools.generators.Validator.ValidationResult.SeverityType;
using arkitektum.gistools.generators.Validator.CircleReferenceValidator;

namespace arkitektum.gistools.generators.Validator
{
    public class UMLValidator
    {
        private readonly Repository _repository;
        private readonly ValidatorSettings _settings;
        
        private readonly List<IValidator> _validators = new List<IValidator>();

        private readonly ModelHasNoCircularReferences _circularReferenceValidator;
        public UMLValidator(Repository repository, ValidatorSettings settings)
        {
            _repository = repository;
            _settings = settings;
            _circularReferenceValidator = new ModelHasNoCircularReferences(_repository);
            LoadValidators();
        }

        private void LoadValidators()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(IValidator).IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

            Log("-----------------------------");
            Log("Laster inn valideringsregler: ");

            foreach (Type type in types)
            {
                var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);
                IValidator validatorInstance = (IValidator) constructors[0].Invoke(new object[] { _repository, _settings });
                _validators.Add(validatorInstance);
                Log("-- [" + validatorInstance.GetRuleId() + "] " + validatorInstance.GetFriendlyName());
            }
            Log("-- [" + _circularReferenceValidator.GetRuleId() + "] " +_circularReferenceValidator.GetFriendlyName());
            Log("-- Lastet inn " + (types.Count() + 1)  +" valideringsregler.");
        }

        public List<string> GetValidators()
        {
            List<string> validatorNames = new List<string>();
            foreach (IValidator validator in _validators)
            {
                validatorNames.Add(validator.GetType().ToString());
            }
            return validatorNames;
        }

        public List<string> GetValidatorsFriendlyName()
        {
            List<string> validatorNames = new List<string>();
            foreach (IValidator validator in _validators)
            {
                validatorNames.Add("[" + validator.GetRuleId() + "] " + validator.GetFriendlyName());
            }
            return validatorNames;
        }

        public bool IsValid(Package packageToValidate)
        {
            var results = RunValidation(packageToValidate);

            return IsValid(results);
        }

        public bool IsValid(List<ValidationResult> validationResults)
        {
            return validationResults.Count() == 0;
        }

        public List<ValidationResult> RunValidation(Package packageToValidate)
        {
            Log("-- Starter validering --");
            var validationStart = DateTime.Now;

            List<ValidationResult> results = Validate(packageToValidate);

            ValidationResult circularRefResult = _circularReferenceValidator.RunValidation(packageToValidate);

            if(circularRefResult != null)
            {
                results.Add(circularRefResult);
                Log(circularRefResult);
            }
            var validationStop = DateTime.Now;

            TimeSpan validationDuration = validationStop.Subtract(validationStart);

            int numberOfErrors = results.Count(r => r.IsSeverity(Error));
            int numberOfWarnings = results.Count(r => r.IsSeverity(Warning));

            Log("--------------------------------------------------");
            Log("-- Validering fullført: " + numberOfErrors + " feil, " + numberOfWarnings + " advarsler.");
            Log("-- Total kjøretid: " + validationDuration.TotalSeconds + " sekunder");
            Log("--------------------------------------------------");

            return results;
        }

        public List<ValidationResult> Validate(Package packageToValidate)
        {
            var validationResults = new List<ValidationResult>();

            validationResults.AddRange(Validate(packageToValidate.Element));

            foreach (Element e in packageToValidate.Elements)
            {
                validationResults.AddRange(Validate(e));
            }

            foreach (Package p in packageToValidate.Packages)
            {
                validationResults.AddRange(Validate(p));
            }

            return validationResults;
        }

        public List<ValidationResult> Validate(Element element)
        {
            var allValidationResults = new List<ValidationResult>();
            foreach (IValidator validator in _validators)
            {
                List<ValidationResult> results = validator.Validate(element);
                if (results.Any())
                {
                    results.ForEach(Log);
                    allValidationResults.AddRange(results);
                }
            }
            return allValidationResults;
        }

        private void Log(ValidationResult validationResult)
        {
            string messagePrefix = "Advarsel";
            if (validationResult.IsSeverity(Error))
            {
                messagePrefix = "FEIL";
            }
            Log("-- [" + validationResult.RuleId + "] " + messagePrefix + ": " + validationResult.ErrorMessage, validationResult.ElementId);
        }

        private void Log(string message, int elementId = 0)
        {
            _repository.WriteOutput("System", "[Regel] " + message, elementId);
        }
    }
}
