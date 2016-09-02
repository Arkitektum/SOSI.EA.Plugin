using System.Collections.Generic;

namespace arkitektum.gistools.generators.Validator.CircleReferenceValidator.TreeModel
{
    public class Node
    {
        private Node _parent;
        private string _parentPointer;
        private NodeIdentifier _identifier;
        private List<Node> _children;

        public Node(NodeIdentifier identifier)
        {
            _identifier = identifier;
            _children = new List<Node>();
        }

        public Node AddChild(Node newNode, string parentPointer)
        {
            newNode._parent = this;
            newNode._parentPointer = parentPointer;

            if (IsSameAsThisOrAncestor(newNode))
                ThrowCircularReferenceException(newNode);

            _children.Add(newNode);

            return newNode;
        }

        private bool IsSameAsThisOrAncestor(Node node)
        {
            return node.IsSameAs(this) || (_parent != null && _parent.IsSameAsThisOrAncestor(node));
        }

        private bool IsSameAs(Node node)
        {
            return _identifier.IsSameIdentifier(node._identifier);
        }

        private void ThrowCircularReferenceException(Node circleTriggerNode)
        {
            var referenceCircle = new List<Reference>();

            // Defines the reference that triggers the circle:
            referenceCircle.Add(circleTriggerNode.CreateReferenceFromParent());

            if (!circleTriggerNode.IsSameAs(this)) // self-reference
                AddReferenceFromParent(referenceCircle, circleTriggerNode);

            throw new CircularReferenceException("Circular reference found.", referenceCircle);
        }

        private void AddReferenceFromParent(List<Reference> referenceCircle, Node circleTriggerNode)
        {
            referenceCircle.Insert(0, CreateReferenceFromParent());

            if (!_parent.IsSameAs(circleTriggerNode))
                _parent.AddReferenceFromParent(referenceCircle, circleTriggerNode);
        }

        private Reference CreateReferenceFromParent()
        {
            string _nameReferencer = _parent._identifier.ToString();
            string _nameReferencePointer = _parentPointer;
            string _nameReference = _identifier.ToString();

            return new Reference(_nameReferencer, _nameReferencePointer, _nameReference);
        }
    }
}
