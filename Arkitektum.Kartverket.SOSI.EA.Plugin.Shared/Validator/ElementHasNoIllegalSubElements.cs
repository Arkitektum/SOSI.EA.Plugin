using EA;
using System.Collections.Generic;
using System;

namespace arkitektum.gistools.generators.Validator
{
    public class ElementHasNoIllegalSubElements : BaseValidator
    {
        public static readonly string MessageTemplate = "Element [{0} {1}] kan ikke være en del av [{2} {3}]";

        private List<string> _stereotypesIllegalForSubElements;

        public override string GetRuleUrl()
        {
            return "https://arkitektum.atlassian.net/wiki/display/gistools/Feilplasserte+elementer";
        }

        public override string GetRuleId()
        {
            return "V1021";
        }
        public ElementHasNoIllegalSubElements(Repository repository, ValidatorSettings settings) : base(repository, settings)
        {
            _stereotypesIllegalForSubElements = new List<string> { "featureType", "dataType", "codeList" };
        }

        public override void RunValidation(Element element)
        {
            foreach (Element subElement in element.Elements)
                if (!IsLegalSubElement(subElement))
                    AddError(subElement, element);
        }

        public override string GetFriendlyName()
        {
            return "Elementer kan ikke være underelementer av andre elementer. - https://arkitektum.atlassian.net/wiki/display/gistools/Feilplasserte+elementer";
        }

        private bool IsLegalSubElement(Element subElement)
        {
            return IsPackage(subElement) || !_stereotypesIllegalForSubElements.Contains(subElement.Stereotype);
        }

        private void AddError(Element subElement, Element element)
        {
            string message = string.Format(MessageTemplate, subElement.Stereotype, subElement.Name, element.Stereotype, element.Name);

            AddError(message, subElement.ElementID);
        }
    }
}
