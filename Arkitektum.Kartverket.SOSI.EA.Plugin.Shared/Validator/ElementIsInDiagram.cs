using EA;

namespace arkitektum.gistools.generators.Validator
{
    public class ElementIsInDiagram : BaseValidator
    {
        public static readonly string MessageTemplate = "Element [{0}] vises ikke i noe diagram.";

        public override string GetRuleUrl()
        {
            return "https://arkitektum.atlassian.net/wiki/display/gistools/Elementer+som+ikke+vises+i+diagram";
        }

        public override string GetRuleId()
        {
            return "V1022";
        }

        public ElementIsInDiagram(Repository repository, ValidatorSettings settings) : base(repository, settings)
        {
        }

        public override void RunValidation(Element element)
        {
            if (IsClass(element))
            {
                string result = Repository.SQLQuery("SELECT Diagram_ID FROM t_diagramobjects where Object_ID=" + element.ElementID);
                if (result == null || !result.Contains("Diagram_ID"))
                {
                    var message = string.Format(MessageTemplate, element.Name);
                    AddWarning(message, element.ElementID);
                }
            }
        }

        public override string GetFriendlyName()
        {
            return "Elementer må vises i et diagram. - https://arkitektum.atlassian.net/wiki/display/gistools/Elementer+som+ikke+vises+i+diagram";
        }
    }
}
