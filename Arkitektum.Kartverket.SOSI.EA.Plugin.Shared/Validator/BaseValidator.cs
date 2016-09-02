using System.Collections.Generic;
using EA;
using System.Linq;
using System.Text.RegularExpressions;

namespace arkitektum.gistools.generators.Validator
{
    public abstract class BaseValidator : IValidator
    {
        protected readonly Repository Repository;
        protected readonly ValidatorSettings Settings;
        protected readonly List<ValidationResult> Results;
        private static readonly string[] GeometryTypes = { "flate", "kurve", "sverm", "punkt" };

        public BaseValidator(Repository repository, ValidatorSettings settings)
        {
            Repository = repository;
            Settings = settings;
            Results = new List<ValidationResult>();
        }
        
        public List<ValidationResult> Validate(Element element)
        {
            Results.Clear();
            RunValidation(element);
            return Results;
        }

        protected void AddWarning(string message, int elementId)
        {
            Results.Add(ValidationResult.CreateWarning(message, elementId, this.GetRuleId(), this.GetRuleUrl()));
        }

        protected void AddError(string message, int elementId)
        {
            Results.Add(ValidationResult.CreateError(message, elementId, this.GetRuleId(), this.GetRuleUrl()));
        }

        protected bool IsPackage(Element element)
        {
            return element.MetaType == "Package";
        }
        protected bool IsApplicationSchemaPackage(Element element)
        {
            return element.Stereotype.ToLower() == "applicationschema";
        }

        protected bool ValidateForGml()
        {
            return Settings.ValidateForGml;
        }

        protected bool ValidateForSosi()
        {
            return Settings.ValidateForSosi;
        }

        protected bool IsGeneralizationConnector(Connector connector)
        {
            return connector.MetaType == "Generalization";
        }

        protected bool IsAssociationConnector(Connector connector)
        {
            return connector.MetaType == "Association";
        }

        protected bool IsAggregationConnector(Connector connector)
        {
            return connector.MetaType == "Aggregation";
        }

        protected bool IsNoteLinkConnector(Connector connector)
        {
            return connector.MetaType == "NoteLink";
        }
        protected bool IsTopo(Connector connector)
        {
            return connector.Stereotype.ToLower() == "topo";
        }
        protected bool IsCodelist(Element element)
        {
            return element.Stereotype != null && element.Stereotype.ToLower() == "codelist";
        }

        protected static bool ClassifierIsPrimitiveType(Attribute attribute)
        {
            return attribute.ClassifierID == 0;
        }

        protected static bool ClassifierIsExternalType(Attribute attribute)
        {
            return !ClassifierIsPrimitiveType(attribute);
        }

        protected bool IsClass(Element element)
        {
            return element.MetaType == "Class";
        }

        protected bool IsEnum(Element element)
        {
            return (element.MetaType.Equals("Enumeration") || element.Stereotype.ToLower() == "enumeration");
        }

        protected bool IsGeometryType(string type)
        {
            return GeometryTypes.Contains(type.ToLower());
        }

        protected bool NameHasNotRecommendedCharacters(string name)
        {
            return new Regex("[-_]").IsMatch(name);
        }

        public abstract void RunValidation(Element element);

        public abstract string GetFriendlyName();

        public abstract string GetRuleUrl();
        public abstract string GetRuleId();
    }
}
