using arkitektum.gistools.generators.Validator.CircleReferenceValidator.TreeModel;

namespace arkitektum.gistools.generators.Validator.CircleReferenceValidator.EATreeModel
{
    public class EANodeIdentifier : NodeIdentifier
    {
        private string _name;
        private int _elementId;

        public EANodeIdentifier(string name, int elementId)
        {
            _name = name;
            _elementId = elementId;
        }
        
        public bool IsSameIdentifier(object obj)
        {
            return _elementId == ((EANodeIdentifier)obj)._elementId;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
