using Arkitektum.EA.Tools;
using EA;
using System;
using Attribute = EA.Attribute;


namespace arkitektum.gistools.generators.Validator
{
    public class AttributeHasValidSosiName : BaseValidator
    {
        public static string MessageTemplateInvalidNCName = "Attributt [{1}] på elementet [{2}] med tagged value SOSI_navn [{0}] inneholder ugyldige tegn (er ikke i henhold til NCName).";

        public override string GetRuleUrl()
        {
            return "https://arkitektum.atlassian.net/wiki/display/gistools/Navneregler";
        }

        public override string GetRuleId()
        {
            return "V1011";
        }
        public AttributeHasValidSosiName(Repository repository, ValidatorSettings settings) : base(repository, settings)
        {
        }

        public override void RunValidation(Element element)
        {
            if (ValidateForSosi() && !IsCodelist(element) && !IsEnum(element))
            {
                foreach (Attribute attribute in element.Attributes)
                {
                    if(IsGeometryType(attribute.Type)) continue;

                    try
                    {
                        string sosiName = RepositoryUtil.GetSosiNameOnly(attribute, Repository);
                        if (string.IsNullOrWhiteSpace(sosiName)) continue;

                        if (!RepositoryUtil.IsNCName(sosiName))
                                AddError(string.Format(MessageTemplateInvalidNCName, sosiName, attribute.Name, element.Name), attribute.AttributeID);
                    }
                    catch (Exception e)
                    {
                        AddError("Ukjent feil ved validering av SOSI_navn: " + e.Message, attribute.AttributeID);
                    }
                }
            }
        }

        public override string GetFriendlyName()
        {
            string friendlyName = "Tagged value SOSI_navn må ha gyldige tegn. (ISO 19103 CSL Rec 10) - https://arkitektum.atlassian.net/wiki/display/gistools/Navneregler";

            if (!ValidateForSosi())
                friendlyName += " (Deaktivert)";

            return friendlyName;
        }
    }
}
