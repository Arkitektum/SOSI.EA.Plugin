namespace arkitektum.gistools.generators.Validator
{
    public class ValidationResult
    {
        public enum SeverityType
        {
            Error,
            Warning
        };

        public SeverityType Severity { get; }
        public int ElementId { get; }
        public string ErrorMessage { get; }

        public string RuleId { get; }

        public string RuleUrl { get; }

        public object GetLogData()
        {
            return new { Severity = Severity, ValidationMessage = ErrorMessage, RuleId = RuleId, RuleUrl = RuleUrl };
        }

        public ValidationResult(SeverityType severity, string errorMessage, int elementId, string ruleId, string ruleUrl)
        {
            Severity = severity;
            ErrorMessage = errorMessage;
            ElementId = elementId;
            RuleId = ruleId;
            RuleUrl = ruleUrl;
        }

        public bool IsSeverity(SeverityType severityToCompare)
        {
            return Severity.Equals(severityToCompare);
        }

        public static ValidationResult CreateError(string message, int elementId, string ruleId, string ruleUrl)
        {
            return new ValidationResult(SeverityType.Error, message, elementId, ruleId, ruleUrl);
        }

        public static ValidationResult CreateWarning(string message, int elementId, string ruleId, string ruleUrl)
        {
            return new ValidationResult(SeverityType.Warning, message, elementId, ruleId, ruleUrl);
        }
    }
}
