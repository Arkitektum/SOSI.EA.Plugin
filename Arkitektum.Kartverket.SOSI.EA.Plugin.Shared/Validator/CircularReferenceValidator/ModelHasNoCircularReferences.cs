using arkitektum.gistools.generators.Validator.CircleReferenceValidator.EATreeModel;
using arkitektum.gistools.generators.Validator.CircleReferenceValidator.TreeModel;
using EA;

namespace arkitektum.gistools.generators.Validator.CircleReferenceValidator
{
    public class ModelHasNoCircularReferences
    {
        Repository _repository;

        public static string MessageTemplate = "Sirkelreferanse: {0}";

        public static string ReferenceTemplate = "[{0}.{1}]->[{2}] ";


        public ModelHasNoCircularReferences(Repository repository)
        {
            _repository = repository;
        }

        public ValidationResult RunValidation(Package package)
        {
            ValidationResult validationResult = null;

            Node root = new Node(new EANodeIdentifier(package.Name, package.Element.ElementID));

            try
            {
                foreach (Element element in package.Elements)
                    AddChildNodeForElement(root, element);
            }
            catch (CircularReferenceException e)
            {
                string circularRefString = "";

                foreach (Reference reference in e.ReferenceCircle)
                {
                    circularRefString += string.Format(ReferenceTemplate,
                                                reference._nameReferencer,
                                                reference._nameReferencePointer,
                                                reference._nameReference);
                }

                string message = string.Format(MessageTemplate, circularRefString);
                validationResult = new ValidationResult(ValidationResult.SeverityType.Error, message, 0, "V1029", "https://arkitektum.atlassian.net/wiki/display/gistools/Sirkelreferanser");
            }

            return validationResult;
        }

        private void AddChildNodeForElement(Node parent, Element element, Attribute referencingAttr = null)
        {
            string parentPointer = (referencingAttr != null) ? referencingAttr.Name : null;

            Node child = parent.AddChild(new Node(new EANodeIdentifier(element.Name, element.ElementID)), parentPointer);


            foreach (Attribute attribute in element.Attributes)
                if (IsComplex(attribute))
                    AddChildNodeForElement(child, _repository.GetElementByID(attribute.ClassifierID), attribute);
        }

        private bool IsComplex(Attribute attribute)
        {
            return attribute.ClassifierID != 0;
        }

        public string GetFriendlyName()
        {
            return "Sirkelreferanser er ikke tillatt. - https://arkitektum.atlassian.net/wiki/display/gistools/Sirkelreferanser";
        }

        public  string GetRuleId()
        {
            return "V1029";
        }
        public string GetRuleUrl()
        {
            return "https://arkitektum.atlassian.net/wiki/display/gistools/Sirkelreferanser";
        }
    }
}
