using System;
using System.Collections.Generic;

namespace arkitektum.gistools.generators.Validator.CircleReferenceValidator
{
    public class CircularReferenceException : Exception
    {
        public List<Reference> ReferenceCircle { get; }

        public CircularReferenceException(string message, List<Reference> referenceCircle) : base(message)
        {
            ReferenceCircle = referenceCircle;
        }
    }
}
