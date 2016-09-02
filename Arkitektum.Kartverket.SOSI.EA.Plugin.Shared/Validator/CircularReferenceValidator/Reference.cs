namespace arkitektum.gistools.generators.Validator.CircleReferenceValidator
{
    public class Reference
    {
        public string _nameReferencer;
        public string _nameReferencePointer;
        public string _nameReference;

        public Reference(string nameReferencer, string nameReferencePointer, string nameReference)
        {
            _nameReferencer = nameReferencer;
            _nameReferencePointer = nameReferencePointer;
            _nameReference = nameReference;
        }
    }
}
