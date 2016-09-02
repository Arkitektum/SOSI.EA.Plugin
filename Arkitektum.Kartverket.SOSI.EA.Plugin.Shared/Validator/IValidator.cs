using System.Collections.Generic;
using EA;

namespace arkitektum.gistools.generators.Validator
{
    /// <summary>
    /// All UML validators must implement this interface. The validation class will be automatically instantiated. 
    /// The constructor must include one parameter, EA.Repository. 
    /// </summary>
    public interface IValidator
    {
        List<ValidationResult> Validate(Element element);

        string GetFriendlyName();

       string GetRuleId();
    }
}
