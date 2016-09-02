using System.Linq;
using EA;

namespace arkitektum.gistools.generators.Validator
{
    public class AttributeHasValidTypeReference : BaseValidator
    {
        public static string MessageTemplate =
            "Ugyldig type [{0}] på attributt [{1}] i element [{2}]. Typen finnes ikke i modellen eller er ugyldig.";

        private static readonly string[] ListOfPrimitiveTypes = {"integer", "characterstring", "real", "date", "datetime", "boolean", "flate", "punkt", "kurve", "sverm", "decimal", "uri", "gm_point", "gm_curve", "gm_surface", "gm_solid", "gm_compositepoint", "gm_compositecurve", "gm_compositesurface", "gm_compositesolid", "gm_multipoint", "gm_multicurve", "gm_multisurface", "gm_multisolid", "gm_object",
            "length", "directposition", "pt_freetext", "vector", "distance", "angle", "speed", "velocity", "scale", "area", "volume", "measure", "unitofmeasure",
            "record", "recordtype", "sign", "time", "year", "any", "dictionary", "gm_primitive", "gm_position", "quantity" };

        public override string GetRuleUrl()
        {
            return "https://arkitektum.atlassian.net/wiki/display/gistools/Ukjent+type";
        }

        public override string GetRuleId()
        {
            return "V1012";
        }

        public AttributeHasValidTypeReference(Repository repository, ValidatorSettings settings) : base(repository, settings)
        {
        }

        public override void RunValidation(Element element)
        {
            if (IsCodelist(element) || IsEnum(element)) return; // TODO: Move to caller(s) as a precondition check(?)
            
            foreach (Attribute attribute in element.Attributes)
            {
                if (ClassifierIsPrimitiveType(attribute))
                {
                    var attributeType = attribute.Type.ToLower();

                    if (!ListOfPrimitiveTypes.Contains(attributeType)) { 
                        AddError(string.Format(MessageTemplate, attribute.Type, attribute.Name, element.Name), attribute.AttributeID);
                    }
                }
            }
        }

        public override string GetFriendlyName()
        {
            return "Attributter har gyldig type. - https://arkitektum.atlassian.net/wiki/display/gistools/Ukjent+type";
        }
    }
}
