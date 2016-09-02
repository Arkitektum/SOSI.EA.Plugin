using System;
using System.Linq;
using Arkitektum.EA.Tools;
using EA;
using Attribute = EA.Attribute;

namespace arkitektum.gistools.generators.Validator
{
    public class AttributeHasSosiName : BaseValidator
    {
        public static string MessageTemplate = "Attributt [{0}] på elementet [{1}] mangler tagged value [SOSI_navn].";

        public override string GetRuleUrl()
        {
            return "https://arkitektum.atlassian.net/wiki/display/gistools/SOSI_Navn+mangler";
        }

        public override string GetRuleId()
        {
            return "V1009";
        }
        public AttributeHasSosiName(Repository repository, ValidatorSettings settings) : base(repository, settings)
        {
        }

        public override void RunValidation(Element element)
        {
            if (!ValidateForSosi() || IsCodelist(element) || IsEnum(element)) return;

            foreach (Attribute attribute in element.Attributes)
            {
                try
                {
                    if (!IsGeometryType(attribute.Type) && !HasSosiName(attribute))
                    {
                        AddError(string.Format(MessageTemplate, attribute.Name, element.Name), attribute.AttributeID);
                    }
                }
                catch (Exception e)
                {
                    AddError("Ukjent feil ved validering av SOSI_navn: " + e.Message, attribute.AttributeID);
                }
            }
        }

        public override string GetFriendlyName()
        {
            string friendlyName = "Attributter må ha SOSI_navn - https://arkitektum.atlassian.net/wiki/display/gistools/SOSI_Navn+mangler";

            if (!ValidateForSosi())
                friendlyName += " (Deaktivert)";

            return friendlyName;
        }

        private bool HasSosiName(Attribute attribute)
        {
            return !string.IsNullOrWhiteSpace(RepositoryUtil.GetSosiNameOnly(attribute, Repository));
        }
    }
}
