using Arkitektum.EA.Tools;
using EA;
using System;
using Attribute = EA.Attribute;


namespace arkitektum.gistools.generators.Validator
{
    public class AttributeSosiNameIsMax32Chars : BaseValidator
    {
        public static string MessageTemplate = "Attributt [{1}] på element [{2}] har tagged value SOSI_navn [{0}] med mer enn 32 tegn.";

        public override string GetRuleUrl()
        {
            //TODO egen url
            return "https://arkitektum.atlassian.net/wiki/display/gistools/Valideringsregler";
        }

        public override string GetRuleId()
        {
            return "V1015";
        }

        public AttributeSosiNameIsMax32Chars(Repository repository, ValidatorSettings settings) : base(repository, settings)
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

                        if (!string.IsNullOrWhiteSpace(sosiName) && sosiName.Length > 32)
                            AddError(string.Format(MessageTemplate, sosiName, attribute.Name, element.Name), attribute.AttributeID);
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
            string friendlyName = "Attributters SOSI_navn kan være på maks. 32 tegn.";

            if (!ValidateForSosi())
                friendlyName += " (Deaktivert)";

            return friendlyName;
        }
    }
}
